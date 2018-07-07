using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using DSharpPlus.Entities;
using Oddity;

namespace InElonWeTrust.Core.Services.Description
{
    public class DescriptionService
    {
        private Timer _descriptionRefreshTimer;
        private OddityCore _oddity;

        private const int InitialIntervalSeconds = 2;
        private const int IntervalMinutes = 30;
        private const string DescriptionPattern = "e!help";
        private const string DescriptionPatternExtended = "e!help | {0} h to launch";

        public DescriptionService()
        {
            // We want to be sure that bot has managed to connect with Discord server, so first interval is quick
            _descriptionRefreshTimer = new Timer(InitialIntervalSeconds * 1000);
            _descriptionRefreshTimer.Elapsed += DescriptionRefreshTimer_Elapsed;

            _oddity = new OddityCore();
        }

        public void Run()
        {
            _descriptionRefreshTimer.Start();
        }

        private async void DescriptionRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var nextLaunch = await _oddity.Launches.GetNext().ExecuteAsync();
            var description = "";

            if (nextLaunch.LaunchDateUtc == null)
            {
                description = DescriptionPattern;
            }
            else
            {
                var hoursToLaunch = Math.Ceiling((nextLaunch.LaunchDateUtc.Value - DateTime.UtcNow).TotalHours);
                description = string.Format(DescriptionPatternExtended, hoursToLaunch);
            }

            await Bot.Client.UpdateStatusAsync(new DiscordGame(description));

            _descriptionRefreshTimer.Interval = IntervalMinutes * 1000 * 60;
        }
    }
}
