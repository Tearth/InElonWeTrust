using System;
using InElonWeTrust.Core;

namespace InElonWeTrust
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Bot().Run().GetAwaiter().GetResult();
            while (Console.ReadLine() != "quit");
        }
    }
}
