using NLog;
using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UMFE.Framework;
using UmfEngine;

namespace Game
{
    internal class Program
    {
        // We're just gonna use 60 FPS because every computer can handle it, and I can't be bothered to separate game logic from rendering.
        // We're also disabling vsync, because in pathological situations, it can cause extreme lag.
        public const double TargetFps = 60;
        public static readonly TimeSpan TargetFrameTime = TimeSpan.FromSeconds(1 / TargetFps);

        public const bool StressTest = true;

        private static Logger Logger;
        private static void ConfigureNLog()
        {
            LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToConsole();
            });
            Logger = LogManager.GetCurrentClassLogger();
        }

        static void Main(string[] args)
        {
            ConfigureNLog();

            var engineConfig = new EngineConfiguration
            {
                HasFixedAspectRatio = true,
                DefaultClearColor = Color.Black,
                DefaultCursorVisible = true,
                DefaultVSync = false,
                DefaultFullscreen = false,
                UseRenderCompatibilityMode = true,
            };

            using var engine = new Engine(engineConfig);

            int frameNumber = 0;
            var funCircleCoord = new Vector2(1, 2);
            while (true)
            {
                var sw = Stopwatch.StartNew();

                var input = engine.GetInput();
                if (input.ShuttingDown || input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_ESCAPE))
                    break;

                if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_F))
                    engine.ToggleFullscreen();

                frameNumber++;

                // game logic here
                // TODO

                // draw calls here
                var c = engine.GetCamera();
                var transform = new AffineTransformation();
                engine.ClearScreen();

                // draw obnoxious lines to see the masking

                // TODO: remove, testing
                //var numx = frameNumber % 100;
                //var obnoxiousTransform = transform.GetTranslated(0.5f * numx, 0);
                var obnoxiousTransform = transform;

                engine.DrawThinLine(c, obnoxiousTransform, Color.Purple, new Vector2(-1000, 18), new Vector2(1000, 18));
                engine.DrawThinLine(c, obnoxiousTransform, Color.Purple, new Vector2(32, -1000), new Vector2(32, 1000));

                var characterTransform = new AffineTransformation();
                characterTransform = characterTransform.GetTranslatedRelativeToWorld(10, 10);
                characterTransform = characterTransform.GetScaled(5);
                characterTransform = characterTransform.GetRotatedAbsoluteRadians(frameNumber / 100f);
                engine.DrawCharacter(c, characterTransform, 'd', Color.GreenYellow);

                // TODO: reuse instead of re-drawing
                var fpsBlock = new TextBlock(null, $"FPS: {engine.FPS:0.00}\nThread utilization: {engine.ThreadUtilization * 100:0.0}%", new Vector2(0, 0), 1f);
                fpsBlock.Draw(engine, c);

                engine.CompleteFrame(TargetFrameTime);

                //Console.WriteLine($"FPS: {engine.FPS:0.00}; thread utilization: {engine.ThreadUtilization * 100:0.0}%");
            }
        }
    }
}
