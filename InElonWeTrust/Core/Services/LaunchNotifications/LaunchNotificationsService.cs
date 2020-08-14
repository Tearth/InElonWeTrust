using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers.Comparers;
using InElonWeTrust.Core.Services.Cache;
using NLog;
using Oddity;
using Oddity.Models.Launches;

namespace InElonWeTrust.Core.Services.LaunchNotifications
{
    public class LaunchNotificationsService
    {
        public event EventHandler<LaunchNotification> OnLaunchNotification;

        private readonly System.Timers.Timer _notificationsUpdateTimer;
        private readonly CacheService _cacheService;
        private readonly List<int> _notificationTimes;
        private LaunchInfo _savedNextLaunchState;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        private const int UpdateNotificationsIntervalMinutes = 1;

        public LaunchNotificationsService(OddityCore oddity, CacheService cacheService)
        {
            _cacheService = cacheService;
            _notificationTimes = new List<int> { 10, 60, 60 * 24, 60 * 24 * 7 };

            _notificationsUpdateTimer = new System.Timers.Timer(UpdateNotificationsIntervalMinutes * 60 * 1000);
            _notificationsUpdateTimer.Elapsed += Notifications_UpdateTimerOnElapsed;
            _notificationsUpdateTimer.Start();

            _cacheService.RegisterDataProvider(CacheContentType.NextLaunch, async p => await oddity.LaunchesEndpoint.GetNext().ExecuteAsync());
        }

        public async Task AddMessageToSubscribe(DiscordChannel channel, DiscordMessage message)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var messageToSubscribe = new MessageToSubscribe(channel.GuildId.ToString(), message.Id.ToString());

                await databaseContext.MessagesToSubscribe.AddAsync(messageToSubscribe);
                await databaseContext.SaveChangesAsync();
            }
        }

        private async void Notifications_UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                await UpdateNotifications();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to update launch notifications");
            }
        }

        private async Task UpdateNotifications()
        {
            if (!await _updateSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                if (_savedNextLaunchState == null)
                {
                    _savedNextLaunchState = await _cacheService.GetAsync<LaunchInfo>(CacheContentType.NextLaunch);
                    return;
                }

                var newLaunchState = await _cacheService.GetAsync<LaunchInfo>(CacheContentType.NextLaunch);
                if (LaunchComparer.IsLaunchTheSame(_savedNextLaunchState, newLaunchState))
                {
                    if (!HasLaunchDateChanged(_savedNextLaunchState, newLaunchState))
                    {
                        if (CheckIfReminderShouldBeSend(newLaunchState))
                        {
                            SendReminderNotification(newLaunchState);
                        }
                    }
                    else
                    {
                        SendScrubNotification(newLaunchState);
                    }
                }
                else
                {
                    SendNewTargetNotification(newLaunchState);
                }

                _savedNextLaunchState = newLaunchState;
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private bool HasLaunchDateChanged(LaunchInfo savedLaunch, LaunchInfo currentLaunch)
        {
            if (savedLaunch.DateUtc == null || currentLaunch.DateUtc == null)
            {
                return false;
            }

            if (savedLaunch.DatePrecision != currentLaunch.DatePrecision)
            {
                return true;
            }

            var oldDate = savedLaunch.DateUtc.Value;
            var newDate = currentLaunch.DateUtc.Value;

            switch (savedLaunch.DatePrecision)
            {
                case DatePrecision.Year:
                case DatePrecision.Half:
                case DatePrecision.Quarter:
                {
                    return oldDate.Year != newDate.Year;
                }

                case DatePrecision.Month:
                {
                    return oldDate.Year != newDate.Year || oldDate.Month != newDate.Month;
                }

                case DatePrecision.Day:
                {
                    return oldDate.Year != newDate.Year || oldDate.Month != newDate.Month || oldDate.Day != newDate.Day;
                }

                // Notify only if difference is bigger than 5 minutes (prevent from sending notifications when date has changed by one minute)
                case DatePrecision.Hour when Math.Abs((oldDate - newDate).TotalMinutes) >= 5:
                {
                    return oldDate.Year != newDate.Year || oldDate.Month != newDate.Month || oldDate.Day != newDate.Day ||
                           oldDate.Hour != newDate.Hour || oldDate.Minute != newDate.Minute;
                }
            }

            return false;
        }

        private bool CheckIfReminderShouldBeSend(LaunchInfo launch)
        {
            if (launch.DatePrecision != DatePrecision.Hour)
            {
                return false;
            }

            var minutesToLaunch = ((launch.DateUtc ?? DateTime.MaxValue) - DateTime.Now.ToUniversalTime()).TotalMinutes;

            var previousStateMinutesToLaunch = minutesToLaunch + 1;
            var newStateMinutesToLaunch = minutesToLaunch;

            return _notificationTimes.Any(p => p < previousStateMinutesToLaunch && p >= newStateMinutesToLaunch);
        }

        private void SendScrubNotification(LaunchInfo newLaunchState)
        {
            OnLaunchNotification?.Invoke(this, new LaunchNotification(LaunchNotificationType.Scrub, _savedNextLaunchState, newLaunchState));
            _logger.Info("Scrub notification sent");
        }

        private void SendReminderNotification(LaunchInfo newLaunchState)
        {
            OnLaunchNotification?.Invoke(this, new LaunchNotification(LaunchNotificationType.Reminder, _savedNextLaunchState, newLaunchState));
            _logger.Info("Reminder notification sent");
        }

        private void SendNewTargetNotification(LaunchInfo newLaunchState)
        {
            OnLaunchNotification?.Invoke(this, new LaunchNotification(LaunchNotificationType.NewTarget, _savedNextLaunchState, newLaunchState));
            _logger.Info("New target notification sent");
        }
    }
}
