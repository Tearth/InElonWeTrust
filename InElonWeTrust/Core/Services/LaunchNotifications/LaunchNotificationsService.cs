using System;
using System.Timers;
using NLog;
using Oddity;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.LaunchNotifications
{
    public class LaunchNotificationsService
    {
        private Timer _notificationsUpdateTimer;
        private OddityCore _oddity;
        private LaunchInfo _nextLaunch;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        private const int IntervalMinutes = 1;

        public LaunchNotificationsService()
        {
            _oddity = new OddityCore();

            _notificationsUpdateTimer = new Timer(IntervalMinutes * 60 * 1000);
            _notificationsUpdateTimer.Elapsed += Notifications_UpdateTimerOnElapsed;
            _notificationsUpdateTimer.Start();
        }

        private async void Notifications_UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_nextLaunch == null)
            {
                _nextLaunch = await _oddity.Launches.GetNext().ExecuteAsync();
            }
            else
            {

            }
        }
    }
}
