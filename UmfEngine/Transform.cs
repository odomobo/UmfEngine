using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public class Transform
    {
        public Transform? Parent { get; }
        private AffineTransformation _affineTransformation = new AffineTransformation();
        public float GetScale { 
            get
            {
                if (Parent == null)
                {
                    return _affineTransformation.Scale;
                }
                else
                {
                    return Parent.GetScale * _affineTransformation.Scale;
                }
            } 
        }

        public Transform()
        {
            Parent = null;
        }

        public Transform(Transform parent)
        {
            Parent = parent;
        }

        public Transform(Transform parent, AffineTransformation affineTransformation)
        {
            Parent = parent;
            _affineTransformation = affineTransformation;
        }

        public Vector2 TransformVector(Vector2 v)
        {
            if (Parent != null)
            {
                v = Parent.TransformVector(v);
            }

            return _affineTransformation.TransformVector(v);
        }

        public void Translate(Vector2 translation)
        {
            _affineTransformation = _affineTransformation.GetTranslated(translation);
        }

        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        public void Rotate(float angle)
        {
            _affineTransformation = _affineTransformation.GetRotated(angle);
        }

        public void Scale(float scale)
        {
            _affineTransformation = _affineTransformation.GetScaled(scale);
        }

        public Transform Clone()
        {
            return new Transform(Parent, _affineTransformation);
        }
    }
}
