using System;
using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Watchdog.Platforms;
using NLog;

namespace InElonWeTrust.Core.Services.Watchdog
{
    public class WatchdogService
    {
        private BaseWatchdog _watchdog;
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected readonly Logger _watchdogLogger = LogManager.GetLogger("WatchdogLogger");

        public WatchdogService()
        {
            var watchdogFactory = new WatchdogFactory();
            _watchdog = watchdogFactory.Create();
        }

        public void Start()
        {
            _watchdog.StartTimer();
            _logger.Info("Watchdog timer started");
        }

        public void Stop()
        {
            _watchdog.StopTimer();
            _logger.Info("Watchdog timer stopped");
        }

        public void ResetApp()
        {
            _logger.Info("Resetting app...");
            _watchdogLogger.Info("Resetting app...");

            _watchdog.ResetApp();
        }
    }
}
