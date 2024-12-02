using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UmfEngine;

namespace Game
{
    internal class Program
    {
        // We're just gonna use 60 FPS because every computer can handle it, and I can't be bothered to separate game logic from rendering.
        // We're also disabling vsync, because in pathological situations, it can cause extreme lag.
        public const float TargetFps = 60;
        public static readonly TimeSpan DeltaTime = TimeSpan.FromSeconds(1 / TargetFps);
        public static readonly float DeltaTimeSeconds = 1 / TargetFps;
        public static readonly Vector2 GravityUnitsPerSecond = new Vector2(0, 20f);
        public static readonly Vector2 GravityPerFrame = GravityUnitsPerSecond * DeltaTimeSeconds;
        private static Logger Logger;
        public static List<IGameObject> FriendlyProjectiles = new List<IGameObject>();

        private static IConfigurationRoot GetAppSettings()
        {
            return new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        private static void ConfigureNLog(IConfigurationRoot appConfiguration)
        {
            LogManager.Setup().LoadConfigurationFromSection(appConfiguration);
        }

        public const float FloorY = 34;

        static void Main(string[] args)
        {
            var appConfiguration = GetAppSettings();
            ConfigureNLog(appConfiguration);
            Logger = LogManager.GetCurrentClassLogger();

            var engineConfig = new EngineConfiguration
            {
                HasFixedAspectRatio = true,
                DefaultClearColor = Color.Black,
                DefaultCursorVisible = false,
                DefaultVSync = false,
                DefaultFullscreen = false,
            };

            using var e = new Engine(engineConfig);
            
            var tank = new Tank { Position = new Vector2(5, FloorY) };

            int frameNumber = 0;
            while (true)
            {
                var sw = Stopwatch.StartNew();

                var input = e.GetInput();
                if (input.ShuttingDown || input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_ESCAPE))
                    break;

                if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_F))
                    e.ToggleFullscreen();

                frameNumber++;

                // game logic here
                var t = e.GetCamera();
                tank.Update(e, t);
                foreach (var projectile in FriendlyProjectiles)
                {
                    projectile.Update(e, t);
                }

                // draw calls here
                e.ClearScreen();

                DrawFloor(e);
                tank.Draw(e, t);

                foreach (var projectile in FriendlyProjectiles)
                {
                    projectile.Draw(e, t);
                }

                DrawCursor(e, input);

                e.CompleteFrame(DeltaTime);
                //engine.CompleteFrame();

                Logger.Info($"FPS: {e.FPS:0.00}; thread utilization: {e.ThreadUtilization*100:0.0}%");
            }
        }

        private static void DrawCursor(Engine engine, Input input)
        {
            var cursorColor = Color.White;

            var c = engine.GetCamera();
            var transform = new Transform();
            var cursorPosition = input.GetMousePosition(c);
            transform.Translate(cursorPosition);
            transform.Scale(1f); // cursor size of 1 unit
            // we probably don't need to rotate
            //transform = transform.GetRotated((float)engineTime.TotalSeconds * 4);
            engine.DrawThinLine(c, transform, cursorColor, new Vector2(-0.5f, 0), new Vector2(0.5f, 0));
            engine.DrawThinLine(c, transform, cursorColor, new Vector2(0, -0.5f), new Vector2(0, 0.5f));
        }

        private static void DrawFloor(Engine e)
        {
            var floorColor = Color.IndianRed;
            var floorThickness = 0.16f;

            var c = e.GetCamera();
            var t = new Transform();
            e.DrawLine(c, t, floorThickness, floorColor, 0, FloorY + floorThickness / 2, 64, FloorY + floorThickness / 2);
        }
    }
}
