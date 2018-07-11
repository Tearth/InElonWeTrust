using System;
using System.Timers;
using DSharpPlus.Entities;
using NLog;
using Oddity;

namespace InElonWeTrust.Core.Services.Description
{
    public class DescriptionService
    {
        private Timer _descriptionRefreshTimer;
        private OddityCore _oddity;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        private const int InitialIntervalSeconds = 5;
        private const int IntervalMinutes = 30;
        private const string DescriptionPattern = "e!help";
        private const string DescriptionPatternExtended = "e!help | {0} h to launch";

        public DescriptionService()
        {
            // We want to be sure that bot has managed to connect with Discord server, so first interval is quick
            _descriptionRefreshTimer = new Timer(InitialIntervalSeconds * 1000);
            _descriptionRefreshTimer.Elapsed += DescriptionRefreshTimer_Elapsed;
            _descriptionRefreshTimer.Start();

            _oddity = new OddityCore();
        }

        private async void DescriptionRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _descriptionRefreshTimer.Interval = IntervalMinutes * 1000 * 60;

            var nextLaunch = await _oddity.Launches.GetNext().ExecuteAsync();
            var description = string.Empty;

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
            _logger.Info($"Description updated: {description}");
        }
    }
}
