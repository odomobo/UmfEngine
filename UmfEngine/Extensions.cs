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

        public static float GetAngle(this Vector2 v)
        {
            v = v / v.Length();

            var angle = MathF.Atan2(v.X, -v.Y);
            if (angle < 0) 
                angle += MathF.Tau;

            return angle;
        }

        public static Vector2 GetRotated(this Vector2 v, float angle)
        {
            float x = v.X * MathF.Cos(angle) + v.Y * -MathF.Sin(angle);
            float y = v.X * MathF.Sin(angle) + v.Y * MathF.Cos(angle);
            return new Vector2(x, y);
        }
    }
}
