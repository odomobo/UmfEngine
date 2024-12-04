﻿using System.Drawing;
using System.Numerics;
using UmfEngine;

namespace Game
{
    internal class FriendlyShell : IGameObject
    {
        public GameObjectTransform Transform;
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
        }
    }
}
