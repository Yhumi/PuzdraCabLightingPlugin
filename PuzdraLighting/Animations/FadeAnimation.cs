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

            var redDifference = Math.Abs(EndColour.Red - StartColour.Red) * lerpValue;
            var greenDifference = Math.Abs(EndColour.Green - StartColour.Green) * lerpValue;
            var blueDifference = Math.Abs(EndColour.Blue - StartColour.Blue) * lerpValue;

            var redMultiplier = (EndColour.Red - StartColour.Red) < 0 ? -1 : 1;
            var greenMultiplier = (EndColour.Green - StartColour.Green) < 0 ? -1 : 1;
            var blueMultiplier = (EndColour.Blue - StartColour.Blue) < 0 ? -1 : 1;

            return new FastIOColour(
                (byte) (StartColour.Red + (redDifference * redMultiplier)), 
                (byte) (StartColour.Green + (greenDifference * greenMultiplier)), 
                (byte) (StartColour.Blue + (blueDifference * blueMultiplier)));
        }

        public override DateTime GetEndTime()
        {
            return StartTime.AddMilliseconds(HoldDelay + Duration);
        }
    }
}
