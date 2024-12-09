using System.Drawing;
using System.Numerics;
using UMFE.Framework;
using UmfEngine;

namespace Game
{
    internal class FriendlyShell : IGameObject
    {
        public GameObjectTransform Transform { get; private set; }
        public Vector2 Velocity;
        public Color Color = Color.Red;
        public float Length = 0.2f;
        public float Thickness = 0.1f;

        public FriendlyShell(Vector2 position, Vector2 velocity)
        {
            Transform = new GameObjectTransform();
            Transform.TranslateRelativeToParent(position);
            Velocity = velocity;
        }

        public void Draw(Engine e, Camera c)
        {
            var velocityNormalized = Velocity.Normalized();
            e.DrawLine(c, Transform, Thickness, Color, Vector2.Zero, -velocityNormalized * Length);
        }

        public void Update(Engine e, Camera c)
        {
            var velocityPerFrame = Velocity * Program.DeltaTimeSeconds;
            Transform.TranslateRelativeToSelf(velocityPerFrame);
            Velocity += Program.GravityPerFrame;

            if (Transform.PositionRelativeToWorld.Y > Program.FloorY)
            {
                Explode(e);
            }
        }

        private void Explode(Engine e)
        {
            Program.FriendlyProjectiles.Remove(this);
            e.PlayAudioClip(Program.HitHurtAudioClip, 1, Program.Random.FloatBetween(0.85f, 1.15f));
            // TODO: create explosion special effect
        }
    }
}
