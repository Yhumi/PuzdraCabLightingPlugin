using PuzdraLighting.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.Animations
{
    internal class AnimationBase
    {
        public DateTime StartTime { get; set; }
        public int LoopDurationSeconds { get; set; }

        public bool FrontLighting { get; set; }
        public bool LeftLighting { get; set; }
        public bool RightLighting { get; set; }

        public virtual FastIOColour CalculateCurrentColourState(DateTime calcTime)
        {
            return new FastIOColour(0, 0, 0);
        }
    }
}
