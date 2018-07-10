using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Database.Models
{
    public class SubscribedChannel
    {
        public int Id { get; set; }
        public string ChannelId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
    }
}
