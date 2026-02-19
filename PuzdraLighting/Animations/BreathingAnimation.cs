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
            int animationTimePoint = msDifference % (LoopDurationSeconds * 1000);
            int highPoint = (LoopDurationSeconds * 1000 / 2) - 1;

            int redSteps = Math.Abs(LowColour.Red - HighColour.Red) / (LoopDurationSeconds / 2);
            int greenSteps = Math.Abs(LowColour.Green - HighColour.Green) / (LoopDurationSeconds / 2);
            int blueSteps = Math.Abs(LowColour.Blue - HighColour.Blue) / (LoopDurationSeconds / 2);

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
