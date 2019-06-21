using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Services.Cache;
using NLog;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.LaunchNotifications
{
    public class LaunchNotificationsService
    {
        public event EventHandler<LaunchNotification> OnLaunchNotification;

        private readonly System.Timers.Timer _notificationsUpdateTimer;
        private readonly CacheService _cacheService;
        private LaunchInfo _nextLaunchState;
        private readonly List<int> _notificationTimes;

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

            _cacheService.RegisterDataProvider(CacheContentType.NextLaunch, async p => await oddity.Launches.GetNext().ExecuteAsync());
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
                _logger.Error(ex, "Unable to update notifications");
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
                if (_nextLaunchState == null)
                {
                    _nextLaunchState = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);
                }
                else
                {
                    var newLaunchState = await _cacheService.Get<LaunchInfo>(CacheContentType.NextLaunch);

                    if (newLaunchState.FlightNumber.Value == _nextLaunchState.FlightNumber.Value || newLaunchState.MissionName == _nextLaunchState.MissionName)
                    {
                        if (newLaunchState.LaunchDateUtc != _nextLaunchState.LaunchDateUtc)
                        {
                            OnLaunchNotification?.Invoke(this, new LaunchNotification(LaunchNotificationType.Scrub, _nextLaunchState, newLaunchState));
                            _logger.Info("Scrub notification sent");
                        }
                        else
                        {
                            var previousStateMinutesToLaunch = (newLaunchState.LaunchDateUtc - DateTime.Now.AddMinutes(-UpdateNotificationsIntervalMinutes).ToUniversalTime()).Value.TotalMinutes;
                            var newStateMinutesToLaunch = (newLaunchState.LaunchDateUtc - DateTime.Now.ToUniversalTime()).Value.TotalMinutes;

                            if (_notificationTimes.Any(p => previousStateMinutesToLaunch >= p && newStateMinutesToLaunch < p))
                            {
                                OnLaunchNotification?.Invoke(this, new LaunchNotification(LaunchNotificationType.Reminder, _nextLaunchState, newLaunchState));
                                _logger.Info("Reminder notification sent");
                            }
                        }
                    }
                    else
                    {
                        OnLaunchNotification?.Invoke(this, new LaunchNotification(LaunchNotificationType.NewTarget, _nextLaunchState, newLaunchState));
                        _logger.Info("New target notification sent");
                    }

                    _nextLaunchState = newLaunchState;
                }
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }
    }
}
