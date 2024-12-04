using NLog;
using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UmfEngine;

namespace CameraTest
{
    internal class Program
    {
        // We're just gonna use 60 FPS because every computer can handle it, and I can't be bothered to separate game logic from rendering.
        // We're also disabling vsync, because in pathological situations, it can cause extreme lag.
        public const double TargetFps = 60;
        public static readonly TimeSpan TargetFrameTime = TimeSpan.FromSeconds(1 / TargetFps);

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
                DefaultCursorVisible = false,
                DefaultVSync = false,
                DefaultFullscreen = false,
                UseRenderCompatibilityMode = true,
            };

            using var engine = new Engine(engineConfig);

            int frameNumber = 0;
            var funCircleCoord = new Vector2(1, 2);
            var c = engine.GetCamera();
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
                c = c.GetTranslated(1f / 60f, 0);

                var transform = new GameObjectTransform();
                engine.ClearScreen();

                // draw obnoxious lines to see the masking

                // TODO: remove, testing
                //var numx = frameNumber % 100;
                //var obnoxiousTransform = transform.GetTranslated(0.5f * numx, 0);
                var quadrantCamera = engine.GetCamera();
                var obnoxiousTransform = transform;

                engine.DrawThinLine(quadrantCamera, obnoxiousTransform, Color.Purple, new Vector2(-1000, 18), new Vector2(1000, 18));
                engine.DrawThinLine(quadrantCamera, obnoxiousTransform, Color.Purple, new Vector2(32, -1000), new Vector2(32, 1000));

                int stride = 1;
                for (int x = 0; x < 64; x += stride)
                {
                    for (int y = 0; y < 36; y += stride)
                    {
                        var color = Color.Blue;
                        if (x == 0 || x == 63 || y == 0 || y == 35)
                        {
                            color = Color.Red;
                        }
                        else
                        {
                            // uncomment to skip all internal squares
                            //continue;
                        }

                        var tmpTransform = new GameObjectTransform(transform);
                        tmpTransform.TranslateRelativeToSelf(x, y);

                        // TODO: remove; testing
                        //var num = frameNumber % 100;
                        //tmpTransform = tmpTransform.GetTranslated(0.5f * num, 0);

                        // draw X
                        engine.DrawThinLine(c, tmpTransform, color, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f));
                        engine.DrawThinLine(c, tmpTransform, color, new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.1f));

                        // draw box
                        engine.DrawThinLine(c, tmpTransform, color, new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.9f));
                        engine.DrawThinLine(c, tmpTransform, color, new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.9f));
                        engine.DrawThinLine(c, tmpTransform, color, new Vector2(0.9f, 0.9f), new Vector2(0.9f, 0.1f));
                        engine.DrawThinLine(c, tmpTransform, color, new Vector2(0.9f, 0.1f), new Vector2(0.1f, 0.1f));

                        engine.DrawCircle(c, tmpTransform, 0, Color.White, 0.5f, 0.5f);

                    }
                }

                funCircleCoord = funCircleCoord + new Vector2(0.001f, 0.001666f);
                engine.DrawCircle(c, transform, 0, Color.LightGreen, funCircleCoord);

                DrawCursor(engine, input);

                engine.CompleteFrame(TargetFrameTime);
            }
        }

        private static void DrawCursor(Engine engine, Input input)
        {
            var cursorColor = Color.White;

            var c = engine.GetCamera();
            var transform = new GameObjectTransform();
            var cursorPosition = input.GetMousePosition(c);
            transform.TranslateRelativeToSelf(cursorPosition);
            transform.Scale(1f); // cursor size of 1 unit
            // we probably don't need to rotate
            //transform = transform.GetRotated((float)engineTime.TotalSeconds * 4);
            engine.DrawThinLine(c, transform, cursorColor, new Vector2(-0.5f, 0), new Vector2(0.5f, 0));
            engine.DrawThinLine(c, transform, cursorColor, new Vector2(0, -0.5f), new Vector2(0, 0.5f));
        }
    }
}
