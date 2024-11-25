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
        public const double TargetFps = 60;
        public static readonly TimeSpan TargetFrameTime = TimeSpan.FromSeconds(1 / TargetFps);
        private static Logger Logger;

        private static IConfigurationRoot GetAppSettings()
        {
            return new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        private static void ConfigureNLog(IConfigurationRoot appConfiguration)
        {
            LogManager.Setup().LoadConfigurationFromSection(appConfiguration);
        }

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

            using var engine = new Engine(engineConfig);

            int frameNumber = 0;
            while (true)
            {
                var sw = Stopwatch.StartNew();

                var input = engine.GetInput();
                if (input.ShuttingDown || input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_ESCAPE))
                    break;

                if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_F))
                    engine.ToggleFullscreen();

                // if engine doesn't have focus, we freeze the engine
                if (engine.WindowHasFocus())
                {
                    frameNumber++;

                    // game logic here
                    // TODO

                    // draw calls here
                    var transform = engine.GetTransform();
                    engine.ClearScreen();

                    // draw obnoxious lines to see the masking

                    // TODO: remove, testing
                    //var numx = frameNumber % 100;
                    //var obnoxiousTransform = transform.GetTranslated(0.5f * numx, 0);
                    var obnoxiousTransform = transform;

                    engine.DrawThinLine(obnoxiousTransform, new Vector2(-1000, 18), new Vector2(1000, 18), Color.Purple);
                    engine.DrawThinLine(obnoxiousTransform, new Vector2(32, -1000), new Vector2(32, 1000), Color.Purple);

                    int stride = 1;
                    for (int x = 0; x < 64; x+=stride)
                    {
                        for (int y = 0; y < 36; y+=stride)
                        {
                            var color = Color.Blue;
                            if (x == 0 || x == 63 || y == 0 || y == 35)
                                color = Color.Red;
                            else
                                continue;

                            var tmpTransform = transform.GetTranslated(x, y);

                            // TODO: remove; testing
                            //var num = frameNumber % 100;
                            //tmpTransform = tmpTransform.GetTranslated(0.5f * num, 0);

                            // draw X
                            engine.DrawThinLine(tmpTransform, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), color);
                            engine.DrawThinLine(tmpTransform, new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.1f), color);

                            // draw box
                            engine.DrawThinLine(tmpTransform, new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.9f), color);
                            engine.DrawThinLine(tmpTransform, new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.9f), color);
                            engine.DrawThinLine(tmpTransform, new Vector2(0.9f, 0.9f), new Vector2(0.9f, 0.1f), color);
                            engine.DrawThinLine(tmpTransform, new Vector2(0.9f, 0.1f), new Vector2(0.1f, 0.1f), color);
                        }
                    }

                    DrawCursor(engine, input);

                    engine.CompleteDraw(TargetFrameTime);

                    Console.WriteLine($"FPS: {engine.FPS:0.00}; thread utilization: {engine.ThreadUtilization*100:0.0}%");
                }
            }
        }

        private static void DrawCursor(Engine engine, Input input)
        {
            var cursorColor = Color.White;

            var transform = engine.GetTransform();
            var cursorPosition = input.GetMousePosition(transform);
            transform = transform.GetTranslated(cursorPosition);
            transform = transform.GetScaled(1f); // cursor size of 1 unit
            // we probably don't need to rotate
            //transform = transform.GetRotated((float)engineTime.TotalSeconds * 4);
            engine.DrawThinLine(transform, new Vector2(-0.5f, 0), new Vector2(0.5f, 0), cursorColor);
            engine.DrawThinLine(transform, new Vector2(0, -0.5f), new Vector2(0, 0.5f), cursorColor);
        }
    }
}
