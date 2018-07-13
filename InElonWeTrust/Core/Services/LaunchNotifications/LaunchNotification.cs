using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.LaunchNotifications
{
    public class LaunchNotification
    {
        public LaunchNotificationType Type { get; }
        public LaunchInfo OldLaunchState { get; }
        public LaunchInfo NewLaunchState { get; }

        public LaunchNotification(LaunchNotificationType type, LaunchInfo oldLaunchState, LaunchInfo newLaunchState)
        {
            Type = type;
            OldLaunchState = oldLaunchState;
            NewLaunchState = newLaunchState;
        }
    }
}
