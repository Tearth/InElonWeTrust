using System;
using System.Diagnostics;
using System.Timers;

namespace InElonWeTrust.Core.Services.Watchdog.Platforms
{
    public class WindowsWatchdog : BaseWatchdog
    {
        private readonly Timer _watchdogTimer;

        public WindowsWatchdog()
        {
            _watchdogTimer = new Timer(WatchdogSeconds * 1000);
            _watchdogTimer.Elapsed += WatchdogTimer_Elapsed;
        }

        public override void StartTimer()
        {
            _watchdogTimer.Start();
        }

        public override void StopTimer()
        {
            _watchdogTimer.Stop();
        }

        public override void ResetApp()
        {
            StartNewElonInstance();
            CloseCurrentElon();
        }

        private void WatchdogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StopTimer();
            ResetApp();
        }

        private void StartNewElonInstance()
        {
            var command = GetCommandToRunElon();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{command}\"",
                    RedirectStandardOutput = false,
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
        }

        private void CloseCurrentElon()
        {
            Environment.Exit(0);
        }
    }
}
