using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public struct Transform
    {
        /*
         * The 3x2 matrix looks like this:
         * 
         *   [ M11 M21 M31 ]
         *   [ M12 M22 M32 ]
         * 
         * The values of the components are something like:
         * 
         *   [ in_X_out_X  in_Y_out_X  X_translation ]
         *   [ in_X_out_Y  in_Y_out_Y  Y_translation ]
         * 
         * Then a vector 2 with components X, Y gets extended with component 1, and then multiplied, like:
         * 
         *   [ M11 M21 M31 ] [ X ]   [ M11 * X + M21 * Y + M31 * 1 ]
         *   [ M12 M22 M32 ] [ Y ] = [ M12 * X + M22 * Y + M32 * 1 ]
         *                   [ 1 ]
         */

        public readonly Matrix3x2 Matrix;
        public readonly float Scale; // this should just represent what the matrix internally represents

        public Transform()
        {
            Matrix = Matrix3x2.Identity;
            Scale = 1.0f;
        }

        public Transform(Matrix3x2 matrix, float scale)
        {
            Matrix = matrix;
            Scale = scale;
        }

        public Vector2 TransformVector(Vector2 v)
        {
            float x = v.X * Matrix.M11 + v.Y * Matrix.M21 + 1 * Matrix.M31;
            float y = v.X * Matrix.M12 + v.Y * Matrix.M22 + 1 * Matrix.M32;
            return new Vector2(x, y);
        }

        public Vector2 InverseTransformVector(Vector2 v)
        {
            /*
             * To inverse transform, what we'll do is undo the translation (performed by M31 and M32), and then multiply by the inverse matrix.
             * 
             * To invert a 2x2 matrix, you do the following:
             * 
             *         -1
             * [ a b ]    = 1 / (ad-bc) [  d -b ]
             * [ c d ]                  [ -c  a ]
             * 
             * The way we can do that, is first perform the multiplication, then divide the components by (ad-bc).
             * 
             * The real values for our matrix:
             * 
             *             -1
             * [ M11 M21 ]    = 1 / (M11*M22-M21*M12) [  M22 -M21 ]
             * [ M12 M22 ]                            [ -M12  M11 ]
             */

            // translate
            float x1 = v.X - 1 * Matrix.M31;
            float y1 = v.Y - 1 * Matrix.M32;

            // multiply
            float x2 = x1 *  Matrix.M22 + y1 * -Matrix.M21;
            float y2 = x1 * -Matrix.M12 + y1 *  Matrix.M11;

            // scale
            x2 = x2 / (Matrix.M11 * Matrix.M22 - Matrix.M21 * Matrix.M12);
            y2 = y2 / (Matrix.M11 * Matrix.M22 - Matrix.M21 * Matrix.M12);

            return new Vector2(x2, y2);
        }

        public Transform GetTranslated(Vector2 v)
        {
            v = TransformVector(v);
            return new Transform(new Matrix3x2(Matrix.M11, Matrix.M12, Matrix.M21, Matrix.M22, v.X, v.Y), Scale);
        }

        public Transform GetTranslated(float x, float y)
        {
            return GetTranslated(new Vector2(x, y));
        }

        public Transform GetRotated(float radians)
        {
            //var rotationMatrix = Matrix3x2.CreateRotation(radians);

            float m11 = Matrix.M11 *  MathF.Cos(radians) + Matrix.M12 * -MathF.Sin(radians);
            float m12 = Matrix.M11 *  MathF.Sin(radians) + Matrix.M12 *  MathF.Cos(radians);
            float m21 = Matrix.M21 *  MathF.Cos(radians) + Matrix.M22 * -MathF.Sin(radians);
            float m22 = Matrix.M21 *  MathF.Sin(radians) + Matrix.M22 *  MathF.Cos(radians);
            return new Transform(new Matrix3x2(m11, m12, m21, m22, Matrix.M31, Matrix.M32), Scale);
        }

        public Transform GetScaled(float scale)
        {
            float s = scale;
            return new Transform(new Matrix3x2(Matrix.M11*s, Matrix.M12*s, Matrix.M21*s, Matrix.M22*s, Matrix.M31, Matrix.M32), Scale*s);
        }
    }
}
