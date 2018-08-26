using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace InElonWeTrust.Core.Services.Watchdog.Platforms
{
    public class LinuxWatchdog : BaseWatchdog
    {
        private readonly Timer _watchdogTimer;
        private int _windowNumber;

        public LinuxWatchdog()
        {
            _watchdogTimer = new Timer(WatchdogSeconds * 1000);
            _watchdogTimer.Elapsed += WatchdogTimer_Elapsed;

            _windowNumber = GetWindowNumber();
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
            SwitchWindow(_windowNumber);
            StartNewElonInstance();

            var newWindowNumber = GetWindowNumber();

            SwitchWindow(_windowNumber);
            CloseCurrentApp();

            _windowNumber = newWindowNumber;
            SwitchWindow(_windowNumber);

            Environment.Exit(0);
        }

        private void WatchdogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            WatchdogLogger.Info("Watchdog timer elapsed");

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
                    FileName = "tmux",
                    Arguments = $"new-window \"{command}\"",
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };

            process.Start();
        }

        private void CloseCurrentApp()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "tmux",
                Arguments = "kill-window",
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();
        }

        private int GetWindowNumber()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "tmux",
                Arguments = "display-message -p #I",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            var processOutput = process.StandardOutput.ReadToEnd();
            return int.Parse(processOutput);
        }

        private void SwitchWindow(int number)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "tmux",
                Arguments = $"select-window -t {number}",
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();
        }
    }
}
