using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using NLog;

namespace InElonWeTrust.Core.Services.Watchdog.Platforms
{
    public abstract class BaseWatchdog
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly Logger WatchdogLogger = LogManager.GetLogger("WatchdogLogger");

        protected const int WatchdogSeconds = 15;

        protected BaseWatchdog()
        {

        }

        public virtual void StartTimer()
        {

        }

        public virtual void StopTimer()
        {

        }

        public virtual void ResetApp()
        {

        }

        protected string GetCommandToRunElon()
        {
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(currentPath, "InElonWeTrust.dll");

            return $"dotnet {fullPath}";
        }
    }
}
