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

        public static double EaseOutQuint(double c)
        {
            return 1 - Math.Pow(1 - c, 5);
        }

        public static double EaseInQuad(double c)
        {
            return c * c;
        }
    }
}
