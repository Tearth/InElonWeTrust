﻿using System;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Services.Cache;
using NLog;
using Oddity;
using Oddity.Models.Launches;

namespace InElonWeTrust.Core.Services.Description
{
    public class DescriptionService
    {
        private readonly Timer _descriptionRefreshTimer;
        private readonly CacheService _cacheService;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int DescriptionUpdateIntervalMinutes = 1;
        private const int DaysHoursEdge = 48;
        private const int HoursMinutesEdge = 60;
        private const string DescriptionPattern = "e!help | no launch time";
        private const string DescriptionPatternExtended = "e!help | {0} to launch";

        public DescriptionService(CacheService cacheService, OddityCore oddity)
        {
            _descriptionRefreshTimer = new Timer(DescriptionUpdateIntervalMinutes * 60 * 1000);
            _descriptionRefreshTimer.Elapsed += DescriptionRefreshTimer_ElapsedAsync;
            _descriptionRefreshTimer.Start();

            _cacheService = cacheService;

            _cacheService.RegisterDataProvider(CacheContentType.NextLaunch, async p => await oddity.LaunchesEndpoint.GetNext().ExecuteAsync());
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

            if (nextLaunch.DateUtc == null ||
                (nextLaunch.DatePrecision != DatePrecision.Hour &&
                 nextLaunch.DatePrecision != DatePrecision.Day))
            {
                description = DescriptionPattern;
            }
            else
            {
                var timeToLaunch = nextLaunch.DateUtc.Value - DateTime.UtcNow;
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
                    else if (timeToLaunch.TotalHours <= DaysHoursEdge)
                    {
                        description = string.Format(DescriptionPatternExtended, GetHoursToLaunch(timeToLaunch) + " h");
                    }
                    else
                    {
                        description = string.Format(DescriptionPatternExtended, GetDaysToLaunch(timeToLaunch) + " days");
                    }
                }
            }

            await Bot.Client.UpdateStatusAsync(new DiscordActivity(description));
        }

        private int GetMinutesToLaunch(TimeSpan time)
        {
            return (int)Math.Ceiling(time.TotalMinutes);
        }

        private int GetHoursToLaunch(TimeSpan time)
        {
            return (int)Math.Ceiling(time.TotalHours);
        }

        private int GetDaysToLaunch(TimeSpan time)
        {
            return (int)Math.Ceiling(time.TotalDays);
        }
    }
}
