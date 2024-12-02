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
    internal class Tank : IGameObject
    {
        //public float X = 1;
        //public float Y = 1;
        public Vector2 Position;
        public float Size = 1;
        public float TurretSize = 0.8f;
        public float TurretRotationSpeedDegreesPerSecond = 90;
        public float TurretRotationSpeedRadiansPerFrame => 
            Engine.DegreesToRadians(TurretRotationSpeedDegreesPerSecond) * Program.DeltaTimeSeconds;
        public Color Color = Color.GreenYellow;
        private float _turretRotationAngle = 0;
        
        // TODO: add logic where it can launch projectiles
        // TODO: add logic where it can charge up

        public void Update(Engine e, CameraViewport t)
        {
            UpdateTurretAngle(e, t);

            // launch shell if clicked
            if (e.Input.WasMouseButtonPressed(MouseButton.Left))
            {
                var shell = new FriendlyShell();
                var shellOffsetFromTurretNormalized = new Vector2(0, -1);
                shellOffsetFromTurretNormalized = shellOffsetFromTurretNormalized.GetRotated(_turretRotationAngle);
                var shellOffsetFromTurret = shellOffsetFromTurretNormalized * TurretSize;

                shell.Position = Position + new Vector2(0, -Size) + shellOffsetFromTurret;
                shell.Velocity = shellOffsetFromTurretNormalized * 25f;
                Program.FriendlyProjectiles.Add(shell);
            }
        }

        private void UpdateTurretAngle(Engine e, CameraViewport t)
        {
            // move to position of tank
            t = t.GetTranslatedInverse(Position.X, Position.Y);
            // move to position of turret
            t = t.GetTranslatedInverse(0, -Size);
            var mousePosition = e.Input.GetMousePosition(t);
            var mouseAngle = mousePosition.GetAngle();

            // calculate angle clockwise from turret to mouse
            var tmpMouseAngle = mouseAngle;
            if (tmpMouseAngle < _turretRotationAngle)
                tmpMouseAngle += MathF.Tau;

            var angleFromTurretToMouse = tmpMouseAngle - _turretRotationAngle;
            if (angleFromTurretToMouse < MathF.PI)
            {
                // turret should rotate clockwise
                if (angleFromTurretToMouse > TurretRotationSpeedRadiansPerFrame)
                    _turretRotationAngle += TurretRotationSpeedRadiansPerFrame;
                else
                    _turretRotationAngle = mouseAngle;
            }
            else
            {
                // turret should rotate counterclockwise
                if (MathF.Tau - angleFromTurretToMouse > TurretRotationSpeedRadiansPerFrame)
                    _turretRotationAngle -= TurretRotationSpeedRadiansPerFrame;
                else
                    _turretRotationAngle = mouseAngle;
            }
        }

        public void Draw(Engine e, CameraViewport t)
        {
            // let's just draw a silly box
            // move to position of tank
            t = t.GetTranslatedInverse(Position.X, Position.Y);
            e.DrawLinesClosed(t, 0.1f, Color,
                -0.5f * Size,  0f,
                -0.5f * Size, -Size,
                 0.5f * Size, -Size,
                 0.5f * Size,  0f);

            // TODO: give the turret some turret tracking

            // let's add a turret on top
            // move to position of turret
            t = t.GetTranslatedInverse(0, -Size);
            t = t.GetRotatedInverse(_turretRotationAngle);
            e.DrawLine(t, 0.1f, Color, Vector2.Zero, new Vector2(0, -TurretSize));
        }
    }
}
