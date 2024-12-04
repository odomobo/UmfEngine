using System.Numerics;

namespace UmfEngine
{
    public class GameObjectTransform
    {
        public GameObjectTransform? Parent { get; }
        private AffineTransformation _affineTransformation = new AffineTransformation();

        public float ScaleRelativeToParent => _affineTransformation.Scale;
        public float ScaleRelativeToWorld { 
            get
            {
                if (Parent == null)
                {
                    return _affineTransformation.Scale;
                }
                else
                {
                    return Parent.ScaleRelativeToWorld * _affineTransformation.Scale;
                }
            } 
        }
        
        public Vector2 PositionRelativeToParent => _affineTransformation.Translation;
        public Vector2 PositionRelativeToWorld => TransformVectorLocalToWorld(Vector2.Zero);

        public float RotationRadiansRelativeToParent => _affineTransformation.RotationRadians;
        public float RotationDegreesRelativeToParent => _affineTransformation.RotationDegrees;
        public float RotationRadiansRelativeToWorld
        {
            get
            {
                if (Parent == null)
                {
                    return RotationRadiansRelativeToParent;
                }

                var ret = Parent.RotationRadiansRelativeToWorld + RotationRadiansRelativeToParent;

                // normalize
                ret = ret % MathF.Tau;
                if (ret < 0)
                    ret += MathF.Tau;

                return ret;
            }
        }
        public float RotationDegreesRelativeToWorld => Engine.RadiansToDegrees(RotationRadiansRelativeToWorld);

        public GameObjectTransform()
        {
            Parent = null;
        }

        public GameObjectTransform(GameObjectTransform? parent)
        {
            Parent = parent;
        }

        public GameObjectTransform(GameObjectTransform? parent, AffineTransformation affineTransformation)
        {
            Parent = parent;
            _affineTransformation = affineTransformation;
        }

        public static implicit operator AffineTransformation(GameObjectTransform transform)
        {
            return transform.ToAffineTransformation();
        }

        public AffineTransformation ToAffineTransformation()
        {
            if (Parent == null)
            {
                return _affineTransformation;
            }
            else
            {
                // recurse
                var parentAffineTransformation = Parent.ToAffineTransformation();
                return parentAffineTransformation.Apply(_affineTransformation);
            }
        }

        public Vector2 TransformVectorLocalToWorld(Vector2 v)
        {
            v = _affineTransformation.TransformVectorLocalToWorld(v);

            if (Parent != null)
            {
                v = Parent.TransformVectorLocalToWorld(v);
            }

            return v;
        }

        public Vector2 TransformVectorWorldToLocal(Vector2 v)
        {
            if (Parent != null)
            {
                v = Parent.TransformVectorWorldToLocal(v);
            }

            v = _affineTransformation.TransformVectorWorldToLocal(v);

            return v;
        }

        public void TranslateRelativeToSelf(Vector2 translation)
        {
            _affineTransformation = _affineTransformation.GetTranslatedRelativeToSelf(translation);
        }

        public void TranslateRelativeToSelf(float x, float y)
        {
            TranslateRelativeToSelf(new Vector2(x, y));
        }

        public void TranslateRelativeToParent(Vector2 translation)
        {
            _affineTransformation = _affineTransformation.GetTranslatedRelativeToWorld(translation);
        }

        public void TranslateRelativeToParent(float x, float y)
        {
            TranslateRelativeToParent(new Vector2(x, y));
        }

        // TODO: I don't even know if I want this, so I'm removing it for now
#if false
        public void TranslateRelativeToWorld(Vector2 translation)
        {
            // this is the same as relative to parent if the parent is the world
            if (Parent != null)
            {
                translation = Parent.InverseTransformVector(translation);
            }

            TranslateRelativeToParent(translation);
        }

        public void TranslateRelativeToWorld(float x, float y)
        {
            TranslateRelativeToWorld(new Vector2(x, y));
        }
#endif

        public void RotateRelativeToSelfRadians(float angleRadians)
        {
            _affineTransformation = _affineTransformation.GetRotatedRelativeToSelfRadians(angleRadians);
        }

        public void RotateRelativeToSelfDegrees(float angleDegrees)
        {
            _affineTransformation = _affineTransformation.GetRotatedRelativeToSelfDegrees(angleDegrees);
        }

        public void SetRotationRelativeToParentRadians(float angleRadians)
        {
            _affineTransformation = _affineTransformation.GetRotatedAbsoluteRadians(angleRadians);
        }

        public void SetRotationRelativeToParentDegrees(float angleDegrees)
        {
            _affineTransformation = _affineTransformation.GetRotatedAbsoluteDegrees(angleDegrees);
        }

        public void Scale(float scale)
        {
            _affineTransformation = _affineTransformation.GetScaled(scale);
        }

        public GameObjectTransform Clone()
        {
            return new GameObjectTransform(Parent, _affineTransformation);
        }
    }
}
