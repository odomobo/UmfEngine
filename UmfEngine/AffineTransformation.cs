using System.Numerics;

namespace UmfEngine
{
    public struct AffineTransformation
    {
        public readonly Vector2 Translation = default;
        public readonly float RotationRadians = 0;
        public readonly float Scale = 1;
        public float ScaleRelativeToParent => Scale;
        public float RotationDegrees => Engine.RadiansToDegrees(RotationRadians);

        public AffineTransformation()
        {

        }

        public AffineTransformation(Vector2 translation = default, float rotationRadians = 0, float scale = 1)
        {
            Translation = translation;
            RotationRadians = rotationRadians;
            Scale = scale;
        }

        public AffineTransformation Apply(AffineTransformation secondTransform)
        {
            // TODO: huh.. what order to apply in????????
            // let's see what happens if we apply secondTransform to this
            var ret = this;
            ret = ret.GetTranslatedRelativeToSelf(secondTransform.Translation);
            ret = ret.GetScaled(secondTransform.Scale);
            ret = ret.GetRotatedRelativeToSelfRadians(secondTransform.RotationRadians);
            return ret;
        }

        public Vector2 TransformVectorLocalToWorld(Vector2 v)
        {
            // rotate, scale, then translate
            v = v.GetRotatedRadians(RotationRadians);
            v = v * Scale;
            v = v + Translation;
            return v;
        }

        public Vector2 TransformVectorWorldToLocal(Vector2 v)
        {
            // inverse of the following: translate, scale, then rotate.
            v = v - Translation;
            v = v / Scale;
            v = v.GetRotatedRadians(-RotationRadians);
            return v;
        }

        public AffineTransformation GetTranslatedRelativeToSelf(Vector2 v)
        {
            v = TransformVectorLocalToWorld(v);
            return new AffineTransformation(v, RotationRadians, Scale);
        }

        public AffineTransformation GetTranslatedRelativeToSelf(float x, float y)
        {
            return GetTranslatedRelativeToSelf(new Vector2(x, y));
        }

        public AffineTransformation GetTranslatedRelativeToWorld(Vector2 v)
        {
            return new AffineTransformation(Translation + v, RotationRadians, Scale);
        }

        public AffineTransformation GetTranslatedRelativeToWorld(float x, float y)
        {
            return GetTranslatedRelativeToWorld(new Vector2(x, y));
        }

        public AffineTransformation GetRotatedRelativeToSelfRadians(float angleRadians)
        {
            float newRotation = RotationRadians + angleRadians;
            newRotation = Engine.NormalizeAngleRadians(newRotation);
            return new AffineTransformation(Translation, newRotation, Scale);
        }

        public AffineTransformation GetRotatedRelativeToSelfDegrees(float angleDegrees)
        {
            return GetRotatedRelativeToSelfRadians(Engine.DegreesToRadians(angleDegrees));
        }

        public AffineTransformation GetRotatedAbsoluteRadians(float angleRadians)
        {
            angleRadians = Engine.NormalizeAngleRadians(angleRadians);
            return new AffineTransformation(Translation, angleRadians, Scale);
        }

        public AffineTransformation GetRotatedAbsoluteDegrees(float angleDegrees)
        {
            return GetRotatedAbsoluteRadians(Engine.DegreesToRadians(angleDegrees));
        }

        public AffineTransformation GetScaled(float scale)
        {
            return new AffineTransformation(Translation, RotationRadians, Scale * scale);
        }
    }
}
