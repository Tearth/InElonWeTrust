﻿using System.Text;
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
using Tweetinvi.Models;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Twitter SpaceX & Elon Musk")]
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

        [Command("subscribetwitter")]
        [Aliases("subtwitter", "st")]
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

        [Command("unsubscribetwitter")]
        [Aliases("unsubtwitter", "ust")]
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

        [Command("randomelontweet")]
        [Aliases("randomet", "ret")]
        [Description("Get random Elon's tweet.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitter.GetRandomTweetAsync(TwitterUserType.ElonMusk);
            await DisplayTweet(ctx.Channel, tweet);
        }

        [Command("randomspacextweet")]
        [Aliases("randomst", "rst")]
        [Description("Get random SpaceX's tweet.")]
        public async Task RandomSpaceXTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitter.GetRandomTweetAsync(TwitterUserType.SpaceX);
            await DisplayTweet(ctx.Channel, tweet);
        }

        [HiddenCommand]
        [Command("reloadcachedtweets")]
        [Aliases("reloadct", "rct")]
        [Description("Reload cached tweets in database.")]
        public async Task ReloadCachedTweets(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.Channel.SendMessageAsync("Reload cached tweets starts");
            _twitter.ReloadCachedTweetsAsync();
            await ctx.Channel.SendMessageAsync("Reload cached tweets finished");
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
