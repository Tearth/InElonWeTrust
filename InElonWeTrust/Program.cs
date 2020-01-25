using System;
using System.Threading.Tasks;
using InElonWeTrust.Core;
using InElonWeTrust.Core.Helpers.Logger;
using NLog;
using NLog.Targets;

namespace InElonWeTrust
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Target.Register<DiscordLogTarget>("Discord");

            var bot = new Bot();
            await bot.Run();

            CommandLoop(bot);
        }

        private static void CommandLoop(Bot bot)
        {
            while (true)
            {
                try
                {
                    var cmd = Console.ReadLine();
                    var split = cmd.Split(' ');
                
                    switch (split[0])
                    {
                        case "quit": return;

                        case "get":
                        {
                            Console.WriteLine($"Custom launch time: {bot.CacheService.GetCustomLaunchTime()}");
                            break;
                        }

                        case "set":
                        {
                            var hours = Int32.Parse(split[1]);
                            var minutes = Int32.Parse(split[2]);
                            var seconds = Int32.Parse(split[3]);

                            bot.CacheService.SetCustomLaunchTime(new TimeSpan(hours, minutes, seconds));

                            Console.WriteLine("Custom launch time set");
                            break;
                        }

                        case "reset":
                        {
                            bot.CacheService.ResetCustomLaunchTime();
                            Console.WriteLine("Custom launch time reset");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Logger.Fatal(unhandledExceptionEventArgs.ExceptionObject);
        }
    }
}
