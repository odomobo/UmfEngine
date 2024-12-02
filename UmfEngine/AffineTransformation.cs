using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public struct AffineTransformation
    {
        public readonly Vector2 Translation = default;
        public readonly float Rotation = 0;
        public readonly float Scale = 1;

        public AffineTransformation()
        {

        }

        public AffineTransformation(Vector2 translation = default, float rotation = 0, float scale = 1)
        {
            Translation = translation;
            Rotation = rotation;
            Scale = scale;
        }

        public Vector2 TransformVector(Vector2 v)
        {
            // rotate, scale, then translate
            v = v.GetRotated(Rotation);
            v = v * Scale;
            v = v + Translation;
            return v;
        }

        public Vector2 InverseTransformVector(Vector2 v)
        {
            // inverse of the following: translate, scale, then rotate.
            v = v - Translation;
            v = v / Scale;
            v = v.GetRotated(-Rotation);
            return v;
        }

        public AffineTransformation GetTranslated(Vector2 v)
        {
            v = TransformVector(v);
            return new AffineTransformation(v, Rotation, Scale);
        }

        public AffineTransformation GetTranslated(float x, float y)
        {
            return GetTranslated(new Vector2(x, y));
        }

        public AffineTransformation GetTranslatedRaw(Vector2 v)
        {
            return new AffineTransformation(Translation + v, Rotation, Scale);
        }

        public AffineTransformation GetRotated(float angle)
        {
            float newRotation = Rotation + angle;
            newRotation = newRotation % MathF.Tau;
            if (newRotation < 0)
                newRotation += MathF.Tau;

            return new AffineTransformation(Translation, newRotation, Scale);
        }

        public AffineTransformation GetScaled(float scale)
        {
            return new AffineTransformation(Translation, Rotation, Scale * scale);
        }
    }
}
