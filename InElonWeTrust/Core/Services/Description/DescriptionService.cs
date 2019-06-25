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

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int DescriptionUpdateIntervalMinutes = 1;
        private const int HoursMinutesEdge = 99;
        private const int WeeksHoursEdge = 144;
        private const string DescriptionPattern = "e!help | no launch date";
        private const string DescriptionPatternExtended = "e!help | {0} to launch";

        public DescriptionService(CacheService cacheService, OddityCore oddity)
        {
            _descriptionRefreshTimer = new Timer(DescriptionUpdateIntervalMinutes * 60 * 1000);
            _descriptionRefreshTimer.Elapsed += DescriptionRefreshTimer_ElapsedAsync;
            _descriptionRefreshTimer.Start();

            _cacheService = cacheService;

            _cacheService.RegisterDataProvider(CacheContentType.NextLaunch, async p => await oddity.Launches.GetNext().ExecuteAsync());
        }

        private async void DescriptionRefreshTimer_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                await UpdateDescriptionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to update description");
            }
        }

        private async Task UpdateDescriptionAsync()
        {
            var nextLaunch = await _cacheService.GetAsync<LaunchInfo>(CacheContentType.NextLaunch);
            string description;

            if (nextLaunch.LaunchDateUtc == null)
            {
                description = DescriptionPattern;
            }
            else
            {
                var timeToLaunch = nextLaunch.LaunchDateUtc.Value - DateTime.UtcNow;
                if (timeToLaunch.TotalMinutes <= 0)
                {
                    description = DescriptionPattern;
                }
                else
                {
                    if (timeToLaunch.TotalMinutes <= HoursMinutesEdge)
                    {
                        description = string.Format(DescriptionPatternExtended, GetMinutesToLaunch(timeToLaunch) + " min");
                    }
                    else if (timeToLaunch.TotalHours <= WeeksHoursEdge)
                    {
                        description = string.Format(DescriptionPatternExtended, GetHoursToLaunch(timeToLaunch) + " h");
                    }
                    else
                    {
                        description = string.Format(DescriptionPatternExtended, GetWeeksToLaunch(timeToLaunch) + " weeks");
                    }
                }
            }

            await Bot.Client.UpdateStatusAsync(new DiscordActivity(description));
        }

        private int GetMinutesToLaunch(TimeSpan time)
        {
            return (int)Math.Max(0, Math.Ceiling(time.TotalMinutes));
        }

        private int GetHoursToLaunch(TimeSpan time)
        {
            return (int)Math.Max(0, Math.Ceiling(time.TotalHours));
        }

        private int GetWeeksToLaunch(TimeSpan time)
        {
            return (int)Math.Max(0, Math.Ceiling(time.TotalHours / 144));
        }
    }
}
