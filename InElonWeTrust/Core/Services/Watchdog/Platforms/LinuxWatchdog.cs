using System;
using System.Diagnostics;
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
            StartProcessWithoutOutputRedirect("tmux", "new-window");
            StartProcessWithoutOutputRedirect("tmux", "send-keys \"dotnet InElonWeTrust.dll\" ENTER");
        }

        private void CloseCurrentApp()
        {
            StartProcessWithoutOutputRedirect("tmux", "kill-window");
        }

        private int GetWindowNumber()
        {
            return int.Parse(StartProcessWithOutputRedirect("tmux", "display-message -p #I"));
        }

        private void SwitchWindow(int number)
        {
            StartProcessWithoutOutputRedirect("tmux", $"select-window -t {number}");
        }

        private void StartProcessWithoutOutputRedirect(string command, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
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

        private string StartProcessWithOutputRedirect(string command, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            return process.StandardOutput.ReadToEnd();
        }
    }
}
