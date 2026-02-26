using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.Hooks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using PuzdraLighting.Animations;
using PuzdraLighting.Data;
using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzdraLighting.LightingControllers
{
    internal class InstanceLighting : IDisposable
    {
        public bool Enabled { get; set; } = false;

        public ushort TerritoryId { get; set; }
        public byte PreviousWeather { get; set; } = 0xFE;
        public byte Weather { get; set; } = 0xFF;
        public bool IsPlayerDead { get; set; } = false;

        public Dictionary<string, AnimationBase> animationStack = new Dictionary<string, AnimationBase>();

        public FastIOColour Base;
        public FastIOColour Bright;
        public FastIOColour Dull;

        public InstanceLighting() 
        {
            Enabled = true;

            Svc.ClientState.TerritoryChanged += OnInstanceChange;
            ActionEffect.ActionEffectEvent += OnActionEvent;
            

            OnInstanceChange(Svc.ClientState.TerritoryType);
            CalculateWeather();
            OnWeatherChange();
        } 

        public void Dispose()
        {
            Svc.ClientState.TerritoryChanged -= OnInstanceChange;
            ActionEffect.ActionEffectEvent -= OnActionEvent;
        }

        public void Tick()
        {
            if (!EzThrottler.Throttle("PuzdraLighting.LightingLoop", 20)) return;

            if (CalculateWeather())
                OnWeatherChange();

            var playerDiedThisFrame = HandleDeath();

            if (playerDiedThisFrame != DeathAnimationState.None)
            {
                var deathFadeAnimation = new FadeAnimation()
                {
                    StartTime = DateTime.Now,
                    HoldDelay = 0,
                    Duration = 2000,

                    FrontLighting = true,
                    LeftLighting = true,
                    RightLighting = true,

                    RunWhenDead = true,
                };

                //On this frame we can start an animation for death
                if (playerDiedThisFrame == DeathAnimationState.Died)
                {
                    Svc.Log.Debug($"Player has died.");
                    deathFadeAnimation.StartColour = Base;
                    deathFadeAnimation.EndColour = ConstantData.Lights_Off;
                }
                    

                if (playerDiedThisFrame == DeathAnimationState.Raising)
                {
                    Svc.Log.Debug($"Player is raising.");
                    deathFadeAnimation.StartColour = ConstantData.Lights_Off;
                    deathFadeAnimation.EndColour = Base;    
                }

                animationStack.Add($"{nameof(InstanceLightingEventType.Raise)}-{DateTime.Now.ToString("HH:mm:ss.fffffff")}", deathFadeAnimation);
            }

            DateTime calcTime = DateTime.Now;

            List<FastIOColour> frontPanelCalculation = new List<FastIOColour>();
            List<FastIOColour> leftPanelCalculations = new List<FastIOColour>();
            List<FastIOColour> rightPanelCalculation = new List<FastIOColour>();

            //Remove expired animations
            animationStack.RemoveAll(x => x.Value.GetEndTime() < calcTime);

            foreach (var anim in animationStack.Values)
            {
                if (IsPlayerDead && !anim.RunWhenDead)
                    continue;

                //Allows for queuing animations based on delays from castbars/effects.
                if (calcTime < anim.StartTime)
                    continue;

                var lightColor = anim.CalculateCurrentColourState(calcTime);

                //No need to add to the averaging stack.
                if (FastIOColour.Equals(Base, lightColor))
                {
                    Svc.Log.Verbose($"[{anim.GetType().Name}] Skipping adding to stacks as {Base} and {lightColor} match.");
                    continue;
                }

                Svc.Log.Verbose($"[{anim.GetType().Name}] Adding {lightColor} to the enabled stacks.");

                if (anim.FrontLighting)
                    frontPanelCalculation.Add(lightColor);

                if (anim.LeftLighting)
                    leftPanelCalculations.Add(lightColor);

                if (anim.RightLighting)
                    rightPanelCalculation.Add(lightColor);
            }

            //Average the colours in the animation list. This is probably fine.
            //If there are no animations playing currently, take the base colour of the phase/instance/whatever or Off if the player is dead.
            P.LightingHelper.WriteRGBColourValues(
                frontPanelCalculation.Count > 0 ? FastIOColour.Average(frontPanelCalculation) : (IsPlayerDead ? ConstantData.Lights_Off : Base),
                leftPanelCalculations.Count > 0 ? FastIOColour.Average(leftPanelCalculations) : (IsPlayerDead ? ConstantData.Lights_Off : Base),
                rightPanelCalculation.Count > 0 ? FastIOColour.Average(rightPanelCalculation) : (IsPlayerDead ? ConstantData.Lights_Off : Base));
        }

        private void OnActionEvent(ECommons.Hooks.ActionEffectTypes.ActionEffectSet set)
        {
            if (set.Action == null) 
                return;

            if (set.Target == null) 
                return;

            if (set.Source == null)
                return;

            //Can use this to eventually handle player casts separately maybe?
            //Unsure.
            var partyIds = Svc.Party.Select(x => x.EntityId);
            if (partyIds.Contains(set.Source.EntityId))
                return;

            //Filtering out autos with no name from the logs.
            if (P.Config.LogUnnamedActions || !String.IsNullOrWhiteSpace(set.Action.Value.Name.ExtractText()))
                Svc.Log.Verbose($"[{set.Source?.Name ?? "Unknown Actor"}] Cast: {set.Action.Value.Name} ({set.Action.Value.RowId})");

            var animation = ConstantData.GetAnimationBaseForAction(TerritoryId, set.Action.Value.RowId, set.Target.GameObjectId, set.TargetEffects.Select(x => x.TargetID).ToList(), Dull, Base, Bright);
            if (animation == null) 
                return;

            animationStack.Add($"{nameof(InstanceLightingEventType.Action)}-{set.Action.Value.RowId}-{DateTime.Now.ToString("HH:mm:ss.fffffff")}", animation);
        }

        private void OnInstanceChange(ushort territoryId)
        {
            Svc.Log.Debug($"Territory Change: {TerritoryId} -> {territoryId}");
            TerritoryId = territoryId;

            Weather = 0xFF;
        }

        public void OnWeatherChange()
        {
            var phaseData = ConstantData.GetPhaseColours(TerritoryId, Weather);

            //Get the current phase colours.
            var previousBase = Base;
            var previousBright = Bright;
            var previousDull = Dull;

            //Set the new phase colours.
            Base = phaseData.baseColour;
            Bright = phaseData.brightColour;
            Dull = phaseData.dullColour;

            Svc.Log.Debug($"Fading between: 0x{previousBase.Red:X2}{previousBase.Green:X2}{previousBase.Blue:X2} -> 0x{Base.Red:X2}{Base.Green:X2}{Base.Blue:X2}. HoldDelay: {phaseData.delay}ms, Duration: {phaseData.duration}ms");

            //Start a phase change animation here? When we have fades sorted.
            animationStack.Add($"{nameof(InstanceLightingEventType.PhaseChange)}-{Weather}-{DateTime.Now.ToString("HH:mm:ss.fffffff")}", new FadeAnimation()
            {
                StartColour = previousBase,
                EndColour = Base,

                StartTime = DateTime.Now,
                HoldDelay = phaseData.delay,
                Duration = phaseData.duration,

                FrontLighting = true,
                LeftLighting = true,
                RightLighting = true
            });
        }

        /// <summary>
        /// Update the current weather state and flag if the weather has changed.
        /// </summary>
        /// <returns>True - If weather has changed.</returns>
        public unsafe bool CalculateWeather()
        {
            var weatherManager = FFXIVClientStructs.FFXIV.Client.Game.WeatherManager.Instance();
            if (weatherManager == null) { return false; }
            var weather = weatherManager->GetCurrentWeather();

            //No change.
            if (Weather == weather) { return false; }

            //Change the weather, then return true 
            PreviousWeather = Weather;
            Weather = weather;

            Svc.Log.Debug($"Weather Change: {PreviousWeather} -> {Weather}");

            return true;
        }

        public unsafe DeathAnimationState HandleDeath() 
        {
            var playerDeadLastFrame = IsPlayerDead;
            IsPlayerDead = Player.IsDead;

            //If they do not match, the player changed state this frame.
            var stateChangedThisFrame = playerDeadLastFrame != IsPlayerDead;

            if (stateChangedThisFrame && IsPlayerDead)
                return DeathAnimationState.Died;

            if (stateChangedThisFrame && !IsPlayerDead)
                return DeathAnimationState.Raising;

            return DeathAnimationState.None;
        }
    }

    public enum InstanceLightingEventType
    {
        Generic,
        PhaseChange,
        Raise,
        Action,
        Test
    }

    public enum AnimationType
    {
        Pulse,
        ThreePointPulse
    }

    public enum DeathAnimationState
    {
        None,
        Died,
        Raising
    }
}
