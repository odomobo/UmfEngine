using System.Drawing;
using System.Numerics;

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

        public static float GetAngleRadians(this Vector2 v)
        {
            v = v / v.Length();

            var angle = MathF.Atan2(v.X, -v.Y);
            if (angle < 0) 
                angle += MathF.Tau;

            return angle;
        }

        public static float GetAngleDegrees(this Vector2 v)
        {
            return Engine.RadiansToDegrees(v.GetAngleRadians());
        }

        public static Vector2 GetRotatedRadians(this Vector2 v, float angleRadians)
        {
            float x = v.X * MathF.Cos(angleRadians) + v.Y * -MathF.Sin(angleRadians);
            float y = v.X * MathF.Sin(angleRadians) + v.Y * MathF.Cos(angleRadians);
            return new Vector2(x, y);
        }

        public static Vector2 GetRotatedDegrees(this Vector2 v, float angleDegrees)
        {
            return GetRotatedRadians(v, Engine.DegreesToRadians(angleDegrees));
        }

        public static Vector2 Normalized(this Vector2 v)
        {
            return v / v.Length();
        }
    }
}
