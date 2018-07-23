using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Services.Subscriptions
{
    public class SubscriptionStatus
    {
        public bool ElonTwitter { get; set; }
        public bool SpaceXTwitter { get; set; }
        public bool Flickr { get; set; }
        public bool Launches { get; set; }
        public bool Reddit { get; set; }
    }
}
