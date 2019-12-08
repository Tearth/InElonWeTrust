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
            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"**Elon Twitter:** {status.ElonTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**SpaceX Twitter:** {status.SpaceXTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**SpaceXFleet Twitter:** {status.SpaceXFleetTwitter.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Flickr:** {status.Flickr.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Launches:** {status.Launches.ConvertToYesNo()}\r\n");
            contentBuilder.Append($"**Reddit:** {status.Reddit.ConvertToYesNo()}");

            return new DiscordEmbedBuilder
            {
                Title = "Notifications status",
                Description = contentBuilder.ToString(),
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }

        public DiscordEmbed BuildMessageOnAdd(SubscriptionType type)
        {
            return new DiscordEmbedBuilder
            {
                Title = ":rocket: Success!",
                Description = _messagesOnAdd[type],
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }

        public DiscordEmbed BuildMessageOnRemove(SubscriptionType type)
        {
            return new DiscordEmbedBuilder
            {
                Title = ":rocket: Success!",
                Description = _messagesOnRemove[type],
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }

        public DiscordEmbed BuildMessageOnAddAll()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":rocket: Success!",
                Description = "All notifications has been enabled.",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }

        public DiscordEmbed BuildMessageOnRemoveAll()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":rocket: Success!",
                Description = "All notifications has been disabled.",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }
    }
}
