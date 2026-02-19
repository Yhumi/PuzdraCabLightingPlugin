using ECommons.DalamudServices;
using ECommons.Throttlers;
using PuzdraLighting.Animations;
using PuzdraLighting.Data;
using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
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

        public Dictionary<InstanceLightingEventType, AnimationBase> animationStack = new Dictionary<InstanceLightingEventType, AnimationBase>();

        public FastIOColour Base;
        public FastIOColour Bright;
        public FastIOColour Dull;

        public InstanceLighting() 
        {
            Enabled = true;

            Svc.ClientState.TerritoryChanged += OnInstanceChange;

            OnInstanceChange(Svc.ClientState.TerritoryType);
            CalculateWeather();
            OnWeatherChange();
        }

        public void Dispose()
        {
        }

        public void Tick()
        {
            if (!EzThrottler.Throttle("PuzdraLighting.LightingLoop", 100)) return;

            if (CalculateWeather())
                OnWeatherChange();

            var playerDiedThisFrame = HandleDeath();

            if (playerDiedThisFrame)
            {
                //On this frame we can start an animation for death
            }

            DateTime calcTime = DateTime.Now;

            List<FastIOColour> frontPanelCalculation = new List<FastIOColour>();
            List<FastIOColour> leftPanelCalculations = new List<FastIOColour>();
            List<FastIOColour> rightPanelCalculation = new List<FastIOColour>();

            foreach (var anim in animationStack.Values)
            {
                //Allows for queuing animations based on delays from castbars.
                if (calcTime < anim.StartTime)
                    continue;

                var lightColor = anim.CalculateCurrentColourState(calcTime);

                if (anim.FrontLighting)
                    frontPanelCalculation.Add(lightColor);

                if (anim.LeftLighting)
                    leftPanelCalculations.Add(lightColor);

                if (anim.RightLighting)
                    rightPanelCalculation.Add(lightColor);
            }

            //For now we're just going to take the first value in the list, eventually we should average the colour but Im lazy.
            //If there are no animations playing currently, take the base colour of the phase/instance/whatever or Off if the player is dead.
            P.LightingHelper.WriteRGBColourValues(
                frontPanelCalculation.Count > 0 ? frontPanelCalculation[0] : (IsPlayerDead ? ConstantData.Lights_Off : Base),
                leftPanelCalculations.Count > 0 ? leftPanelCalculations[0] : (IsPlayerDead ? ConstantData.Lights_Off : Base),
                rightPanelCalculation.Count > 0 ? rightPanelCalculation[0] : (IsPlayerDead ? ConstantData.Lights_Off : Base));
        }

        private void OnDamageTaken()
        {

        }

        private void OnInstanceChange(ushort territoryId)
        {
            TerritoryId = territoryId;
        }

        private void OnWeatherChange()
        {
            var phaseData = ConstantData.GetPhaseColours(TerritoryId, Weather);

            //Set the phase colours.
            Base = phaseData.baseColour;
            Bright = phaseData.brightColour;
            Dull = phaseData.dullColour;

            //Start a phase change animation here? When we have fades sorted.

        }

        /// <summary>
        /// Update the current weather state and flag if the weather has changed.
        /// </summary>
        /// <returns>True - If weather has changed.</returns>
        private unsafe bool CalculateWeather()
        {
            var weatherManager = FFXIVClientStructs.FFXIV.Client.Game.WeatherManager.Instance();
            if (weatherManager == null) { return false; }
            var weather = weatherManager->GetCurrentWeather();

            //No change.
            if (Weather == weather) { return false; }

            //Change the weather, then return true 
            PreviousWeather = Weather;
            Weather = weather;

            return true;
        }

        public unsafe bool HandleDeath() 
        {
            var playerDeadLastFrame = IsPlayerDead;
            IsPlayerDead = Player.IsDead;

            //If they do not match, and the player is dead, the player died this frame
            return IsPlayerDead && playerDeadLastFrame != IsPlayerDead;
        }
    }

    public enum InstanceLightingEventType
    {
        Generic,
        Raise
    }
}
