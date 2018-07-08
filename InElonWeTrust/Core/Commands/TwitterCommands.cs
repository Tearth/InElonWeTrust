﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Settings;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Twitter")]
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

        [Command("addtwitterchannel")]
        [Aliases("addtc", "atc")]
        [Description("Get random Elon's tweet.")]
        public async Task AddTwitterChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.AddSubscription(ctx.Channel.Id, SubscriptionType.Twitter))
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

        [Command("removetwitterchannel")]
        [Aliases("removetc", "rtc")]
        [Description("Get random Elon's tweet.")]
        public async Task RemoveTwitterChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.RemoveSubscription(ctx.Channel.Id, SubscriptionType.Twitter))
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

        [Command("randomspacextweet")]
        [Aliases("randomst", "rst")]
        [Description("Get random SpaceX's tweet.")]
        public async Task RandomSpaceXTweet(CommandContext ctx)
        {
            var tweet = _twitter.GetRandomTweet(TwitterUserType.SpaceX);
            await DisplayTweet(ctx.Channel, tweet);
        }

        private async Task DisplayTweet(DiscordChannel channel, ITweet tweet)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = tweet.CreatedBy.ProfileImageUrl400x400,
                ImageUrl = tweet.Media.Count > 0 ? tweet.Media[0].MediaURL : ""
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(HttpUtility.HtmlDecode(tweet.FullText));
            contentBuilder.Append("\r\n\r\n");
            contentBuilder.Append(tweet.Url);

            embed.AddField($"{tweet.CreatedBy.Name} at {tweet.CreatedAt}", contentBuilder.ToString());

            await channel.SendMessageAsync("", false, embed);
        }

        private async void Twitter_OnOnNewTweet(object sender, ITweet tweet)
        {
            var channels = _subscriptions.GetSubscribedChannels(SubscriptionType.Twitter);
            foreach (var channelID in channels)
            {
                var channel = await Bot.Client.GetChannelAsync(channelID);
                await DisplayTweet(channel, tweet);
            }
        }
    }
}
