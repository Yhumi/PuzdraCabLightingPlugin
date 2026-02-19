using ECommons.DalamudServices;
using ECommons.Throttlers;
using PuzdraLighting.Animations;
using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.LightingControllers
{
    internal class InstanceLighting : IDisposable
    {
        public bool Enabled { get; set; } = false;

        public Dictionary<InstanceLightingEventType, AnimationBase> animationStack = new Dictionary<InstanceLightingEventType, AnimationBase>();
        
        public InstanceLighting() 
        {
            Enabled = true;
        }

        public void Dispose()
        {
        }

        public void Tick()
        {
            if (!EzThrottler.Throttle("PuzdraLighting.LightingLoop", 100)) return;

            if (Player.IsDead)
            {
                P.LightingHelper.WriteRGBColourValues(
                    new FastIOColour(0x0, 0x0, 0x0),
                    new FastIOColour(0x0, 0x0, 0x0),
                    new FastIOColour(0x0, 0x0, 0x0));
                return;
            }

            DateTime calcTime = DateTime.Now;

            List<FastIOColour> frontPanelCalculation = new List<FastIOColour>();
            List<FastIOColour> leftPanelCalculations = new List<FastIOColour>();
            List<FastIOColour> rightPanelCalculation = new List<FastIOColour>();

            foreach (var anim in animationStack.Values)
            {
                var lightColor = anim.CalculateCurrentColourState(calcTime);

                if (anim.FrontLighting)
                    frontPanelCalculation.Add(lightColor);

                if (anim.LeftLighting)
                    leftPanelCalculations.Add(lightColor);

                if (anim.RightLighting)
                    rightPanelCalculation.Add(lightColor);
            }

            //For now we're just going to take the first value in the list, eventually we should average the colour but Im lazy.
            P.LightingHelper.WriteRGBColourValues(
                frontPanelCalculation.Count > 0 ? frontPanelCalculation[0] : new FastIOColour(0xFF, 0x0, 0xFF),
                leftPanelCalculations.Count > 0 ? leftPanelCalculations[0] : new FastIOColour(0xFF, 0x0, 0xFF),
                rightPanelCalculation.Count > 0 ? rightPanelCalculation[0] : new FastIOColour(0xFF, 0x0, 0xFF));
        }

        private void OnDamageTaken()
        {

        }
    }

    public enum InstanceLightingEventType
    {
        Generic,
        Raise
    }
}
