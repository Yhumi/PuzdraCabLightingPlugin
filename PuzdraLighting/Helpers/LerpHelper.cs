using System;
using System.Collections.Generic;
using System.Text;

namespace PuzdraLighting.Helpers
{
    internal static class LerpHelper
    {
        public static double EaseInOutSine(double c)
        {
            return -(Math.Cos(Math.PI * c) - 1) / 2;
        }
    }
}
