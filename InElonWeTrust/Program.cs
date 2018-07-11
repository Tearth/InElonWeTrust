using System;
using System.Threading.Tasks;
using InElonWeTrust.Core;

namespace InElonWeTrust
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            await new Bot().Run();
            while (Console.ReadLine() != "quit");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
