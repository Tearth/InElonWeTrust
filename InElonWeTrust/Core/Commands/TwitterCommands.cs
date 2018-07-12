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
    [Commands(":newspaper2:", "Twitter", "[SpaceX](https://twitter.com/SpaceX) & [Elon Musk](https://twitter.com/elonmusk)")]
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

        [Command("SubscribeTwitter")]
        [Aliases("SubTwitter", "st")]
        [Description("Subscribe SpaceX & Elon Musk Twitter profiles (bot will post all new tweets).")]
        public async Task AddTwitterChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.AddSubscriptionAsync(ctx.Channel.Id, SubscriptionType.Twitter))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!",
                    "Channel has been added to the Twitter subscription list. Now I will display " +
                    "all posts from @elonmusk and @SpaceX twitter.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Error!",
                    "Channel is already subscribed.");
            }

            await ctx.RespondAsync("", false, embed);
        }

        [Command("UnsubscribeTwitter")]
        [Aliases("UnsubTwitter", "ust")]
        [Description("Removes SpaceX & Elon Musk Twitter profiles subscription.")]
        public async Task RemoveTwitterChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.RemoveSubscriptionAsync(ctx.Channel.Id, SubscriptionType.Twitter))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!",
                    "Channel has been removed the Twitter subscription list.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Error!",
                    "Channel is already removed.");
            }

            await ctx.RespondAsync("", false, embed);
        }

        [Command("IsTwitterSubscribed")]
        [Aliases("TwitterSubscribed", "its")]
        [Description("Get random photo from SpaceX Flickr profile.")]
        public async Task IsTwitterChannelSubscribed(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.Twitter))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Twitter subscription status!",
                    "Channel is subscribed.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Twitter subscription status!",
                    "Channel is not subscribed.");
            }

            await ctx.RespondAsync("", false, embed);
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
