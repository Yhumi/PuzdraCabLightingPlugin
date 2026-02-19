using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.Animations
{
    internal class BreathingAnimation : AnimationBase
    {
        public FastIOColour LowColour { get; set; }
        public FastIOColour HighColour { get; set; }

        public override FastIOColour CalculateCurrentColourState(DateTime calcTime)
        {
            int msDifference = (int) (calcTime - StartTime).TotalMilliseconds;
            double animationTimePoint = msDifference % (Duration * 1000);
            double highPoint = (Duration * 1000 / 2) - 1;

            double redSteps = Math.Abs(LowColour.Red - HighColour.Red) / (Duration / 2);
            double greenSteps = Math.Abs(LowColour.Green - HighColour.Green) / (Duration / 2);
            double blueSteps = Math.Abs(LowColour.Blue - HighColour.Blue) / (Duration / 2);

            if (animationTimePoint > highPoint)
            {
                byte red = (byte) (HighColour.Red - redSteps * (animationTimePoint - highPoint));
                byte green = (byte) (HighColour.Green - greenSteps * (animationTimePoint - highPoint));
                byte blue = (byte) (HighColour.Blue - blueSteps * (animationTimePoint - highPoint));

                return new FastIOColour(red, green, blue);
            }
            else
            {
                byte red = (byte) (LowColour.Red + redSteps * animationTimePoint);
                byte green = (byte) (LowColour.Green + greenSteps * animationTimePoint);
                byte blue = (byte) (LowColour.Blue + blueSteps * animationTimePoint);

                return new FastIOColour(red, green, blue);
            }
        }
    }
}
