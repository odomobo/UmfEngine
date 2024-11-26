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
    internal class Tank
    {
        public float X;
        public float Y;
        public float Size = 1;
        public float TurretSize = 0.8f;
        public Color Color = Color.GreenYellow;

        // TODO: add logic where it can launch projectiles
        // TODO: add logic where it can charge up

        public void Draw(Engine e, Transform t)
        {
            // let's just draw a silly box
            t = t.GetTranslated(X, Y);
            e.DrawLinesClosed(t, 0.1f, Color,
                -0.5f * Size,  0f,
                -0.5f * Size, -Size,
                 0.5f * Size, -Size,
                 0.5f * Size,  0f);

            // TODO: give the turret some turret tracking

            // let's add a turret on top
            t = t.GetTranslated(0, -Size);
            var mousePosition = e.Input.GetMousePosition(t);
            var mousePositionNormalized = mousePosition / mousePosition.Length();
            e.DrawLine(t, 0.1f, Color, Vector2.Zero, mousePositionNormalized * TurretSize);
        }
    }
}
