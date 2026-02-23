using ECommons.DalamudServices;
using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.Animations
{
    internal class FadeAnimation : AnimationBase
    {
        public FastIOColour StartColour { get; set; }
        public FastIOColour EndColour { get; set; }

        public double HoldDelay { get; set; }

        public override FastIOColour CalculateCurrentColourState(DateTime calcTime)
        {
            var trueStart = StartTime.AddMilliseconds(HoldDelay);
            if (calcTime < trueStart)
            {
                Svc.Log.Verbose($"[FadeAnimation] Current Calc Time: {calcTime}. Holding until: {trueStart}");
                return StartColour;
            }

            var timeElapsed = (calcTime - trueStart).TotalMilliseconds;
            var timeElapsedFraction = timeElapsed / Duration;

            var lerpValue = LerpHelper.EaseInOutSine(timeElapsedFraction);

            var lerpRed = (EndColour.Red - StartColour.Red) * lerpValue + StartColour.Red;
            var lerpGreen = (EndColour.Green - StartColour.Green) * lerpValue + StartColour.Green;
            var lerpBlue = (EndColour.Blue - StartColour.Blue) * lerpValue + StartColour.Blue;

            return new FastIOColour(
                (byte)lerpRed,
                (byte)lerpGreen,
                (byte)lerpBlue);
        }

        public override DateTime GetEndTime()
        {
            return StartTime.AddMilliseconds(HoldDelay + Duration);
        }
    }
}
