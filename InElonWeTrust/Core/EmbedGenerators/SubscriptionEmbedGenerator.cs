using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class SubscriptionEmbedGenerator
    {
        private readonly Dictionary<SubscriptionType, string> _messagesOnAdd;
        private readonly Dictionary<SubscriptionType, string> _messagesOnRemove;

        public SubscriptionEmbedGenerator()
        {
            _messagesOnAdd = new Dictionary<SubscriptionType, string>
            {
                {SubscriptionType.ElonTwitter, "Twitter has been subscribed! Now bot will post all newest tweets from [Elon Musk](https://twitter.com/elonmusk) profile."},
                {SubscriptionType.SpaceXTwitter, "Twitter has been subscribed! Now bot will post all newest tweets from [SpaceX](https://twitter.com/SpaceX) profile."},
                {SubscriptionType.Flickr, "Flickr has been subscribed! Now bot will post all newest photos from [SpaceX](https://www.flickr.com/photos/spacex/) profile"},
                {SubscriptionType.NextLaunch, "Launch notifications has been subscribed! Now bot will post all newest information about upcoming launch."}
            };

            _messagesOnRemove = new Dictionary<SubscriptionType, string>
            {
                {SubscriptionType.ElonTwitter, "Elon Twitter subscription has been removed."},
                {SubscriptionType.SpaceXTwitter, "SpaceX Twitter subscription has been removed."},
                {SubscriptionType.Flickr, "Flickr subscription has been removed."},
                {SubscriptionType.NextLaunch, "Launch notifications subscription has been removed."}
            };
        }

        public DiscordEmbedBuilder BuildStatus(SubscriptionStatus status)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"**Elon Twitter:** {status.ElonTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**SpaceX Twitter:** {status.SpaceXTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Flickr:** {status.Flickr.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Launches:** {status.Launches.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Reddit:** {status.Reddit.ConvertToYesNo()}");

            embed.AddField("Notifications status", contentBuilder.ToString());

            return embed;
        }

        public DiscordEmbedBuilder BuildMessageOnAdd(SubscriptionType type)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", _messagesOnAdd[type]);
            return embed;
        }

        public DiscordEmbedBuilder BuildMessageOnRemove(SubscriptionType type)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", _messagesOnRemove[type]);
            return embed;
        }
    }
}
