using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.Animations
{

    internal class PulseAnimation : AnimationBase
    {
        public double PeakPointMs { get; set; }

        public FastIOColour StartEndColour { get; set; }
        public FastIOColour PeakColour { get; set; }

        private DateTime PeakTime { get { return StartTime.AddMilliseconds(PeakPointMs); } }

        public override FastIOColour CalculateCurrentColourState(DateTime calcTime)
        {
            if (calcTime <= PeakTime)
            {
                // Do initial calculation to peak.
                var timeElapsed = (calcTime - StartTime).TotalMilliseconds;
                var timeElapsedFraction = timeElapsed / PeakPointMs;

                var lerpValue = LerpHelper.EaseOutQuint(timeElapsedFraction);

                var redDifference = Math.Abs(StartEndColour.Red - PeakColour.Red) * lerpValue;
                var greenDifference = Math.Abs(StartEndColour.Green - PeakColour.Green) * lerpValue;
                var blueDifference = Math.Abs(StartEndColour.Blue - PeakColour.Blue) * lerpValue;

                return new FastIOColour(
                    (byte)(StartEndColour.Red + redDifference),
                    (byte)(StartEndColour.Green + greenDifference),
                    (byte)(StartEndColour.Blue + blueDifference));
            }
            else
            {
                //Fade down from peak back to base
                var timeElapsed = (calcTime - PeakTime).TotalMilliseconds;
                var timeElapsedFraction = timeElapsed / (Duration - PeakPointMs);

                var lerpValue = 1 - LerpHelper.EaseInQuad(timeElapsedFraction);

                var redDifference = Math.Abs(StartEndColour.Red - PeakColour.Red) * lerpValue;
                var greenDifference = Math.Abs(StartEndColour.Green - PeakColour.Green) * lerpValue;
                var blueDifference = Math.Abs(StartEndColour.Blue - PeakColour.Blue) * lerpValue;

                return new FastIOColour(
                    (byte)(StartEndColour.Red + redDifference),
                    (byte)(StartEndColour.Green + greenDifference),
                    (byte)(StartEndColour.Blue + blueDifference));
            }
        }
    }
}
