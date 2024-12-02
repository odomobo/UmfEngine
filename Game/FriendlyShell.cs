using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UmfEngine;

namespace Game
{
    internal class FriendlyShell : IGameObject
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color = Color.Red;
        public float Length = 0.2f;
        public float Thickness = 0.1f;

        public void Draw(Engine e, CameraViewport t)
        {
            t = t.GetTranslatedInverse(Position);
            var velocityNormalized = Velocity / Velocity.Length();
            e.DrawLine(t, Thickness, Color, Vector2.Zero, -velocityNormalized * Length);
        }

        public void Update(Engine e, CameraViewport t)
        {
            var velocityPerFrame = Velocity * Program.DeltaTimeSeconds;
            Position += velocityPerFrame;
            Velocity += Program.GravityPerFrame;
        }
    }
}
