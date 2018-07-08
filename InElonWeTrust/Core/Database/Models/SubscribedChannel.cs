using System;
using System.Collections.Generic;
using System.Text;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Database.Models
{
    public class SubscribedChannel
    {
        public int ID { get; set; }
        public string ChannelID { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
    }
}
