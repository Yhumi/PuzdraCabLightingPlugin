using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.Animations
{

    internal class ThreePointPulseAnimation : AnimationBase
    {
        public double PeakPointMs { get; set; }

        public FastIOColour StartColour { get; set; }
        public FastIOColour PeakColour { get; set; }
        public FastIOColour EndColour { get; set; }

        private DateTime PeakTime { get { return StartTime.AddMilliseconds(PeakPointMs); } }

        public override FastIOColour CalculateCurrentColourState(DateTime calcTime)
        {
            if (calcTime <= PeakTime)
            {
                // Do initial calculation to peak.
                var timeElapsed = (calcTime - StartTime).TotalMilliseconds;
                var timeElapsedFraction = timeElapsed / PeakPointMs;

                var lerpValue = LerpHelper.EaseOutQuint(timeElapsedFraction);

                var lerpRed = (PeakColour.Red - StartColour.Red) * lerpValue + StartColour.Red;
                var lerpGreen = (PeakColour.Green - StartColour.Green) * lerpValue + StartColour.Green;
                var lerpBlue = (PeakColour.Blue - StartColour.Blue) * lerpValue + StartColour.Blue;

                return new FastIOColour(
                    (byte)lerpRed,
                    (byte)lerpGreen,
                    (byte)lerpBlue);
            }
            else
            {
                //Fade down from peak back to base
                var timeElapsed = (calcTime - PeakTime).TotalMilliseconds;
                var timeElapsedFraction = timeElapsed / (Duration - PeakPointMs);

                var lerpValue = LerpHelper.EaseInQuad(timeElapsedFraction);

                var lerpRed = (EndColour.Red - PeakColour.Red) * lerpValue + PeakColour.Red;
                var lerpGreen = (EndColour.Green - PeakColour.Green) * lerpValue + PeakColour.Green;
                var lerpBlue = (EndColour.Blue - PeakColour.Blue) * lerpValue + PeakColour.Blue;

                return new FastIOColour(
                    (byte)lerpRed,
                    (byte)lerpGreen,
                    (byte)lerpBlue);
            }
        }
        public override DateTime GetEndTime()
        {
            return StartTime.AddMilliseconds(Duration);
        }
    }
}
