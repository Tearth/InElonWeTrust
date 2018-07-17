namespace InElonWeTrust.Core.Database.Models
{
    public class UserLaunchSubscription
    {
        public int Id { get; set; }
        public uint LaunchId { get; set; }
        public string UserId { get; set; }

        public UserLaunchSubscription()
        {

        }

        public UserLaunchSubscription(uint launchId, string userId)
        {
            LaunchId = launchId;
            UserId = userId;
        }
    }
}
