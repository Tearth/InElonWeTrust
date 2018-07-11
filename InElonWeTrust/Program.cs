using System;
using System.Threading.Tasks;
using InElonWeTrust.Core;
using InElonWeTrust.Core.Helpers;
using NLog;
using NLog.Targets;

namespace InElonWeTrust
{
    internal class Program
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Target.Register<DiscordLogTarget>("Discord");

            await new Bot().Run();
            while (Console.ReadLine() != "quit");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Logger.Fatal(unhandledExceptionEventArgs.ExceptionObject);
        }
    }
}
