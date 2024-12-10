using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UMFE.Framework;
using UmfEngine;

namespace Game
{
    internal class Tank : IGameObject
    {
        // TODO: naming
        public GameObjectTransform Transform { get; private set; }
        private readonly float TurretSize = 0.8f;
        private readonly Color Color = Color.GreenYellow;

        private readonly Turret _turret;
        
        // TODO: add logic where it can launch projectiles
        // TODO: add logic where it can charge up

        public Tank(GameObjectTransform? parent, Vector2 position)
        {
            Transform = new GameObjectTransform(parent);
            Transform.TranslateRelativeToParent(position);

            var turretTransform = new GameObjectTransform(Transform);
            turretTransform.Scale(TurretSize);
            turretTransform.TranslateRelativeToParent(0, -1);

            _turret = new Turret(turretTransform);
        }

        public void Update(Engine e, Camera c)
        {
            _turret.Update(e, c);
        }

        public void Draw(Engine e, Camera c)
        {
            // created with designer
            var tankShape = new Vector2[] {
                new Vector2(-1.1999998f, -1f),
                new Vector2(-0.99999976f, -1.2f),
                new Vector2(-0.09999993f, -1.2f),
                new Vector2(0.19999999f, -0.7f),
                new Vector2(1.3000002f, -0.7f),
                new Vector2(1.4000002f, -0.8f),
                new Vector2(1.8000002f, -0.8f),
                new Vector2(2.1f, -0.5f),
                new Vector2(2.1f, -0.3f),
                new Vector2(1.8000002f, 0f),
                new Vector2(-1.0999998f, 0f),
                new Vector2(-1.1999998f, -0.1f),
            };

            e.DrawLinesClosed(c, Transform, 0.1f, Color,
                tankShape);

            _turret.Draw(e, c);
        }

        internal class Turret : IGameObject
        {
            public GameObjectTransform Transform { get; private set; }
            public float TurretSize = 1.5f;
            public float TurretRotationSpeedDegreesPerSecond = 15;
            public float TurretRotationSpeedRadiansPerFrame =>
                Engine.DegreesToRadians(TurretRotationSpeedDegreesPerSecond) * Program.DeltaTimeSeconds;
            public Color Color = Color.GreenYellow;

            public Turret(GameObjectTransform transform)
            {
                Transform = transform;
            }

            // TODO: add logic where it can launch projectiles
            // TODO: add logic where it can charge up

            public void Update(Engine e, Camera c)
            {
                UpdateTurretAngle(e, c);

                // launch shell if clicked
                if (e.Input.WasMouseButtonPressed(MouseButton.Left))
                {
                    Fire(e);
                }
            }

            private void Fire(Engine e)
            {
                // use transform for rotation, and get world position and world rotation to determine how to launch projectile
                // start at the end of the barrel
                var friendlyShellPosition = Transform.TransformVectorLocalToWorld(new Vector2(0, -1));
                var friendlyShellInitialVelocityNormalized = new Vector2(0, -1).GetRotatedRadians(Transform.RotationRadiansRelativeToWorld);
                var friendlyShellInitialVelocity = friendlyShellInitialVelocityNormalized * 25f;

                var shell = new FriendlyShell(friendlyShellPosition, friendlyShellInitialVelocity);
                Program.FriendlyProjectiles.Add(shell);

                e.PlayAudioClip(Program.LaserShootAudioClip, 1, Program.Random.FloatBetween(0.85f, 1.15f));
            }

            private void UpdateTurretAngle(Engine e, Camera c)
            {
                if (e.Input.WasMouseButtonPressed(MouseButton.Right))
                {
                    Debugger.Break();
                }

                var mousePosition = e.Input.GetMousePosition(c);
                var mouseAngle = (mousePosition - Transform.PositionRelativeToWorld).GetAngleRadians();
                mouseAngle = Engine.NormalizeAngleRadians(mouseAngle);
                var turretAngle = Transform.RotationRadiansRelativeToWorld;
                turretAngle = Engine.NormalizeAngleRadians(turretAngle);

                // TODO: we should have turret angle be encoded by the turret game object transform rotating

                // calculate angle clockwise from turret to mouse
                var tmpMouseAngle = mouseAngle;
                if (tmpMouseAngle < turretAngle)
                    tmpMouseAngle += MathF.Tau;

                var angleFromTurretToMouse = tmpMouseAngle - turretAngle;
                if (angleFromTurretToMouse < MathF.PI)
                {
                    // turret should rotate clockwise
                    if (angleFromTurretToMouse > TurretRotationSpeedRadiansPerFrame)
                        turretAngle += TurretRotationSpeedRadiansPerFrame;
                    else
                        turretAngle = mouseAngle;
                }
                else
                {
                    // turret should rotate counterclockwise
                    if (MathF.Tau - angleFromTurretToMouse > TurretRotationSpeedRadiansPerFrame)
                        turretAngle -= TurretRotationSpeedRadiansPerFrame;
                    else
                        turretAngle = mouseAngle;
                }

                Transform.SetRotationRelativeToParentRadians(turretAngle);
            }

            public void Draw(Engine e, Camera c)
            {
                // draw the line vertically, and depend on the transform rotation to point the correct direction
                e.DrawLine(c, Transform, 0.1f, Color, Vector2.Zero, new Vector2(0, -TurretSize));
            }
        }
    }
}
