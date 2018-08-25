using System;
using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Watchdog.Platforms;

namespace InElonWeTrust.Core.Services.Watchdog
{
    public class WatchdogService
    {
        private BaseWatchdog _watchdog;

        public WatchdogService()
        {
            var watchdogFactory = new WatchdogFactory();
            _watchdog = watchdogFactory.Create();
        }

        public void Start()
        {
            _watchdog.StartTimer();
        }

        public void Stop()
        {
            _watchdog.StopTimer();
        }

        public void ResetApp()
        {
            _watchdog.ResetApp();
        }
    }
}
