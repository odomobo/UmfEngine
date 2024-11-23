using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UmfEngine;

namespace Game
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new EngineConfiguration
            {
                HasFixedAspectRatio = true,
            };

            using var engine = new Engine(config);
            float targetFps = 144f;
            var targetFrameTime = TimeSpan.FromSeconds(1 / targetFps);

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

                    TimeSpan engineTime = TimeSpan.FromSeconds(frameNumber / (double)targetFps);

                    // game logic here
                    // TODO

                    // draw calls here
                    var transform = engine.GetTransform();
                    engine.ClearScreen();

                    //var screenSize = engine.GetScreenDimensionsInUnits();
                    //var centerTransform = transform.GetTranslated(new Vector2(screenSize.X / 2, 12));

                    // draw obnoxious lines to see the masking
                    engine.DrawThinLine(transform, new Vector2(-1000, 18), new Vector2(1000, 18), Color.Purple);
                    engine.DrawThinLine(transform, new Vector2(32, -1000), new Vector2(32, 1000), Color.Purple);

                    for (int x = 0; x < 64; x++)
                    {
                        for (int y = 0; y < 36; y++)
                        {
                            var color = Color.Blue;
                            if (x == 0 || x == 63 || y == 0 || y == 35)
                                color = Color.Red;

                            var tmpTransform = transform.GetTranslated(x, y);

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

                    engine.CompleteDraw();
                }

                var frameTime = sw.Elapsed;
                // if we took longer than the allotted frame time, then we don't try to wait
                if (frameTime < targetFrameTime)
                {
                    //Console.WriteLine($"Sleeping for {targetFrameTime - frameTime}");
                    Thread.Sleep(targetFrameTime - frameTime);
                }
            }
        }
    }
}
