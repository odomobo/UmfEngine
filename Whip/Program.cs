﻿using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UmfEngine;

namespace Whip
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new EngineConfiguration
            {
                Title = "Whip",
                ScreenSizeInUnits = 24,
                AllowResize = true,
                DefaultResolution = new Vector2(1920, 1080),
                DefaultFullscreen = false,
                DefaultCursorVisible = false,
            };

            using var engine = new Engine(config);
            engine.ClearColor = Color.Black;
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

                    var screenSize = engine.GetScreenDimensionsInUnits();

                    var centerTransform = transform.GetTranslated(new Vector2(screenSize.X / 2, 2));
                    centerTransform = centerTransform.GetRotated(MathF.PI / 2);
                    DrawSpiral(engine, centerTransform, engineTime);

                    var mousePosition = input.GetMousePosition(transform);

                    //if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_PAUSE))
                    //    Debugger.Break();

                    DrawCursor(engine, transform, input.GetMousePosition(transform), engineTime);

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

        static void DrawCursor(Engine engine, Transform transform, Vector2 cursorPosition, TimeSpan engineTime)
        {
            transform = transform.GetTranslated(cursorPosition);
            transform = transform.GetScaled(0.3f);
            transform = transform.GetRotated((float)engineTime.TotalSeconds*4);
            engine.DrawThinLine(transform, new Vector2(-0.5f, 0), new Vector2(0.5f, 0), Color.White);
            engine.DrawThinLine(transform, new Vector2(0, -0.5f), new Vector2(0, 0.5f), Color.White);
        }

        static void DrawSpiral(Engine engine, Transform transform, TimeSpan engineTime)
        {
            var scale = Math.Sin((engineTime.TotalMilliseconds / 4000));
            transform = transform.GetScaled(1 + (float)scale * 0.1f);
            for (int i = 0; i < 6000; i++)
            {
                var color = Engine.ColorFromHSV((float)engineTime.TotalMilliseconds * 0.1f + i * 0.1f, 0.8f, 1);
                float xOffsetStart = MathF.Sin(-(float)engineTime.TotalMilliseconds * 0.004f + i * 0.3f)*0.02f;
                float xOffsetEnd = MathF.Sin(-(float)engineTime.TotalMilliseconds * 0.004f + (i+1) * 0.3f)*0.02f;

                engine.DrawThinLine(transform, new Vector2(xOffsetStart, 0), new Vector2(xOffsetEnd, -0.8f), color);
                transform = transform.GetTranslated(new Vector2(0, -0.8f));
                //var angle = Math.Cos((engineTime.TotalMilliseconds / 4000));
                var angle = 0;
                transform = transform.GetRotated(0.09f * (1 + (float)angle*0.15f));
                transform = transform.GetScaled(0.9995f);
            }
        }
    }
}