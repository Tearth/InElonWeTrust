using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Extensions;
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
                {SubscriptionType.ElonTwitter, "Elon Musk's Twitter notifications has been enabled! Now bot will post here all newest tweets from [Elon Musk](https://twitter.com/elonmusk) profile."},
                {SubscriptionType.SpaceXTwitter, "SpaceX Twitter notifications has been enabled! Now bot will post here all newest tweets from [SpaceX](https://twitter.com/SpaceX) profile."},
                {SubscriptionType.SpaceXFleetTwitter, "SpaceXFleet Twitter notifications has been enabled! Now bot will post here all newest tweets from [SpaceXFleet](https://twitter.com/SpaceXFleet) profile."},
                {SubscriptionType.Flickr, "Flickr notifications has been enabled! Now bot will post here all newest photos from [SpaceX](https://www.flickr.com/photos/spacex/) profile."},
                {SubscriptionType.Reddit, "Reddit notifications has been enabled! Now bot will post here all newest photos from [/r/spacex](https://www.reddit.com/r/spacex/)."},
                {SubscriptionType.NextLaunch, "Launch notifications has been enabled! Now bot will post here all newest information about upcoming launch."}
            };

            _messagesOnRemove = new Dictionary<SubscriptionType, string>
            {
                {SubscriptionType.ElonTwitter, "Elon Twitter notifications has been disabled."},
                {SubscriptionType.SpaceXTwitter, "SpaceX Twitter notifications has been disabled."},
                {SubscriptionType.SpaceXFleetTwitter, "SpaceXFleet Twitter notifications has been disabled."},
                {SubscriptionType.Flickr, "Flickr notifications has been disabled."},
                {SubscriptionType.Reddit, "Reddit notifications has been disabled."},
                {SubscriptionType.NextLaunch, "Launch notifications has been disabled."}
            };
        }

        public DiscordEmbed BuildStatus(SubscriptionStatus status)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"**Elon Twitter:** {status.ElonTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**SpaceX Twitter:** {status.SpaceXTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**SpaceXFleet Twitter:** {status.SpaceXFleetTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Flickr:** {status.Flickr.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Launches:** {status.Launches.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Reddit:** {status.Reddit.ConvertToYesNo()}");

            embed.AddField("Notifications status", contentBuilder.ToString());

            return embed;
        }

        public DiscordEmbed BuildMessageOnAdd(SubscriptionType type)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", _messagesOnAdd[type]);
            return embed;
        }

        public DiscordEmbed BuildMessageOnRemove(SubscriptionType type)
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
