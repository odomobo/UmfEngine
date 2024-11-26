using NLog;
using SDL;
using UmfEngine;

namespace AudioTest
{
    internal class Program
    {
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

            var rand = new Random();
            var engineConfiguration = new EngineConfiguration
            {

            };
            using var engine = new Engine(engineConfiguration);

            using var hitHurtAudioClip = engine.LoadAudioClip("hitHurt.wav");
            
            while (true)
            {
                var input = engine.GetInput();
                if (input.ShuttingDown || input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_ESCAPE))
                    return;

                if (input.WasMouseButtonPressed(MouseButton.Left))
                {
                    engine.PlayAudioClip(hitHurtAudioClip, 1, rand.FloatBetween(0.9f, 1.1f));
                }
                
                engine.ClearScreen();
                engine.CompleteFrame(TimeSpan.FromSeconds(1d/60));
            }
        }
    }
}
