using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Settings;
using NLog;
using Tweetinvi.Models;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Media)]
    public class TwitterCommands
    {
        private readonly TwitterService _twitterService;
        private readonly SubscriptionsService _subscriptionsService;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TwitterCommands(TwitterService twitterService, SubscriptionsService subscriptionsService)
        {
            _twitterService = twitterService;
            _subscriptionsService = subscriptionsService;

            _twitterService.OnNewTweet += Twitter_OnNewTweet;
        }

        [Command("RandomElonTweet")]
        [Aliases("ElonTweet", "ret")]
        [Description("Get random Elon's tweet.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.ElonMusk);
            await DisplayTweet(ctx.Channel, tweet);
        }

        [Command("RandomSpaceXTweet")]
        [Aliases("SpaceXTweet", "rst")]
        [Description("Get random SpaceX's tweet.")]
        public async Task RandomSpaceXTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.SpaceX);
            await DisplayTweet(ctx.Channel, tweet);
        }

        [HiddenCommand]
        [Command("ReloadTwitterCache")]
        [Description("Reload cached tweets in database.")]
        public async Task ReloadTwitterCache(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _twitterService.ReloadCachedTweetsAsync();
        }

        private async Task DisplayTweet(DiscordChannel channel, CachedTweet tweet)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = tweet.AvatarUrl,
                ImageUrl = tweet.ImageUrl
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(HttpUtility.HtmlDecode(tweet.FullText));
            contentBuilder.Append("\r\n\r\n");
            contentBuilder.Append(tweet.Url);

            embed.AddField($"Twitter: {tweet.CreatedByDisplayName} at {tweet.CreatedAt.ToUniversalTime()} UTC", contentBuilder.ToString());

            await channel.SendMessageAsync("", false, embed);
        }

        private async void Twitter_OnNewTweet(object sender, ITweet tweet)
        {
            var subscriptionType = SubscriptionType.ElonTwitter;
            switch (tweet.CreatedBy.ScreenName)
            {
                case "elonmusk": subscriptionType = SubscriptionType.ElonTwitter;
                    break;

                case "SpaceX": subscriptionType = SubscriptionType.SpaceXTwitter;
                    break;
            }

            var channels = _subscriptionsService.GetSubscribedChannels(subscriptionType);
            foreach (var channelId in channels)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(channelId);
                    await DisplayTweet(channel, new CachedTweet(tweet));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send tweet to the channel with id {channelId}");
                }
            }
        }
    }
}
