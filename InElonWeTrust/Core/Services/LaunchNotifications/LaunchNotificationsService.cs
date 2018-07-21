using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using NLog;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.LaunchNotifications
{
    public class LaunchNotificationsService
    {
        public event EventHandler<LaunchNotification> OnLaunchNoification;

        private Timer _notificationsUpdateTimer;
        private OddityCore _oddity;
        private LaunchInfo _nextLaunchState;
        private List<int> _notificationTimes;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        private const int IntervalMinutes = 1;

        public LaunchNotificationsService()
        {
            _oddity = new OddityCore();
            _notificationTimes = new List<int> { 2, 10, 60, 60 * 24, 60 * 24 * 7 };

            _notificationsUpdateTimer = new Timer(IntervalMinutes * 60 * 1000);
            _notificationsUpdateTimer.Elapsed += Notifications_UpdateTimerOnElapsed;
            _notificationsUpdateTimer.Start();
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
            if (_nextLaunchState == null)
            {
                _nextLaunchState = await _oddity.Launches.GetNext().ExecuteAsync();
            }
            else
            {
                var newLaunchState = await _oddity.Launches.GetNext().ExecuteAsync();

                if (newLaunchState.FlightNumber.Value == _nextLaunchState.FlightNumber.Value)
                {
                    if (newLaunchState.LaunchDateUtc != _nextLaunchState.LaunchDateUtc)
                    {
                        OnLaunchNoification?.Invoke(this, new LaunchNotification(LaunchNotificationType.Scrub, _nextLaunchState, newLaunchState));
                        _logger.Info($"Scrub notification sent to {OnLaunchNoification.GetInvocationList().Length} channels");
                    }
                    else
                    {
                        var previousStateMinutesToLaunch = (newLaunchState.LaunchDateUtc - DateTime.Now.AddMinutes(-IntervalMinutes).ToUniversalTime()).Value.TotalMinutes;
                        var newStateMinutesToLaunch = (newLaunchState.LaunchDateUtc - DateTime.Now.ToUniversalTime()).Value.TotalMinutes;

                        if (_notificationTimes.Any(p => previousStateMinutesToLaunch >= p && newStateMinutesToLaunch < p))
                        {
                            OnLaunchNoification?.Invoke(this, new LaunchNotification(LaunchNotificationType.Reminder, _nextLaunchState, newLaunchState));
                            _logger.Info($"Reminder notification sent to {OnLaunchNoification.GetInvocationList().Length} channels");
                        }
                    }
                }
                else
                {
                    OnLaunchNoification?.Invoke(this, new LaunchNotification(LaunchNotificationType.NewTarget, _nextLaunchState, newLaunchState));
                    _logger.Info($"New target notification sent to {OnLaunchNoification.GetInvocationList().Length} channels");
                }

                _nextLaunchState = newLaunchState;
            }
        }
    }
}
