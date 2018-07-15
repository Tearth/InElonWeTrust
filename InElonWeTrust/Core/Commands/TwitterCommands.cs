using System.Text;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Settings;
using Tweetinvi.Models;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":frame_photo:", "Media", "Commands related with Twitter, Flickr and Reddit")]
    public class TwitterCommands
    {
        private TwitterService _twitter;
        private SubscriptionsService _subscriptions;

        public TwitterCommands()
        {
            _twitter = new TwitterService();
            _subscriptions = new SubscriptionsService();

            _twitter.OnNewTweet += Twitter_OnOnNewTweet;
        }

        [Command("RandomElonTweet")]
        [Aliases("ElonTweet", "ret")]
        [Description("Get random Elon's tweet.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitter.GetRandomTweetAsync(TwitterUserType.ElonMusk);
            await DisplayTweet(ctx.Channel, tweet);
        }

        [Command("RandomSpaceXTweet")]
        [Aliases("SpaceXTweet", "rst")]
        [Description("Get random SpaceX's tweet.")]
        public async Task RandomSpaceXTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitter.GetRandomTweetAsync(TwitterUserType.SpaceX);
            await DisplayTweet(ctx.Channel, tweet);
        }

        [HiddenCommand]
        [Command("ReloadCachedTweetsAsync")]
        [Description("Reload cached tweets in database.")]
        public async Task ReloadCachedTweetsAsync(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _twitter.ReloadCachedTweetsAsync();
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

            embed.AddField($"{tweet.CreatedByDisplayName} at {tweet.CreatedAt}", contentBuilder.ToString());

            await channel.SendMessageAsync("", false, embed);
        }

        private async void Twitter_OnOnNewTweet(object sender, ITweet tweet)
        {
            var channels = _subscriptions.GetSubscribedChannels(SubscriptionType.Twitter);
            foreach (var channelId in channels)
            {
                var channel = await Bot.Client.GetChannelAsync(channelId);
                await DisplayTweet(channel, new CachedTweet(tweet));
            }
        }
    }
}
