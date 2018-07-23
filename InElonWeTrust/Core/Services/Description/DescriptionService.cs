using System;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Services.Cache;
using NLog;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.Description
{
    public class DescriptionService
    {
        private readonly Timer _descriptionRefreshTimer;
        private readonly CacheService _cacheService;
        private readonly OddityCore _oddity;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int IntervalMinutes = 1;
        private const string DescriptionPattern = "e!help";
        private const string DescriptionPatternExtended = "e!help | {0} to launch";

        public DescriptionService(CacheService cacheService, OddityCore oddity)
        {
            _descriptionRefreshTimer = new Timer(IntervalMinutes * 60 * 1000);
            _descriptionRefreshTimer.Elapsed += DescriptionRefreshTimer_Elapsed;
            _descriptionRefreshTimer.Start();

            _cacheService = cacheService;
            _oddity = oddity;

            _cacheService.RegisterDataProvider(CacheContentType.NextLaunch, async p => await _oddity.Launches.GetNext().ExecuteAsync());
        }

        private async void DescriptionRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await UpdateDescription();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to update description");
            }
        }

        private async Task UpdateDescription()
        {
            var nextLaunch = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);
            string description;

            if (nextLaunch.LaunchDateUtc == null)
            {
                description = DescriptionPattern;
            }
            else
            {
                var timeToLaunch = nextLaunch.LaunchDateUtc.Value - DateTime.UtcNow;
                if (timeToLaunch.TotalMinutes <= 99)
                {
                    description = string.Format(DescriptionPatternExtended, GetMinutesToLaunch(timeToLaunch) + " min");
                }
                else
                {
                    description = string.Format(DescriptionPatternExtended, GetHoursToLaunch(timeToLaunch) + " h");
                }
            }

            await Bot.Client.UpdateStatusAsync(new DiscordGame(description));
        }

        private int GetMinutesToLaunch(TimeSpan time)
        {
            return (int)Math.Max(0, Math.Ceiling(time.TotalMinutes));
        }

        private int GetHoursToLaunch(TimeSpan time)
        {
            return (int)Math.Max(0, Math.Ceiling(time.TotalHours));
        }
    }
}
