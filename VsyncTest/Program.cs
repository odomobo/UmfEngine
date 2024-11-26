using NLog;
using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UmfEngine;

namespace VsyncTest
{
    internal class Program
    {
        // we're just gonna use 60 FPS because every computer can handle it, and I can't be bothered to separate game logic from rendering.
        public const float TargetFps = 144f;
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
                //DefaultClearColor = Color.Black,
                DefaultCursorVisible = true,
                DefaultVSync = true,
                DefaultFullscreen = false,
            };

            using var engine = new Engine(engineConfig);

            //engine.CompleteDraw();
            int drawCount = 0;
            var startTime = DateTime.Now;
            var endTime = startTime + TargetFrameTime*99.5;
            while (DateTime.Now < endTime)
            {
                engine.CompleteFrame();
                drawCount++;
            }
            Console.WriteLine($"Drew {drawCount} in {DateTime.Now - startTime}");

            // let's test thread.sleep
            for (int i = 1; i < 20; i++)
            {
                var sw2 = new Stopwatch();
                var sw = Stopwatch.StartNew();
                Thread.Sleep(TimeSpan.FromMilliseconds(16.6667));
                var elapsed = sw.Elapsed;
                Console.WriteLine($"Tried to sleep for {16.6667} ms; instead slept for {elapsed.TotalMilliseconds}");
            }
        }
    }
}
