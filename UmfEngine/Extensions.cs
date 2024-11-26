using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public static class Extensions
    {
        public static (float hue, float saturation, float value) ToHSV(this Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            float hue = color.GetHue();
            float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            float value = max / 255f;
            return (hue, saturation, value);
        }

        public static double DoubleBetween(this Random random, double min, double max)
        {
            var randomDouble = random.NextDouble();
            var diff = max - min;
            return min + (randomDouble * diff);
        }

        public static float FloatBetween(this Random random, float min, float max)
        {
            return (float)random.DoubleBetween(min, max);
        }
    }
}
