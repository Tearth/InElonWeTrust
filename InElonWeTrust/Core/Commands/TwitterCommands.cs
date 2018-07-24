﻿using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
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
        private readonly TwitterEmbedGenerator _twitterEmbedGenerator;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TwitterCommands(TwitterService twitterService, SubscriptionsService subscriptionsService, TwitterEmbedGenerator twitterEmbedGenerator)
        {
            _twitterService = twitterService;
            _subscriptionsService = subscriptionsService;
            _twitterEmbedGenerator = twitterEmbedGenerator;

            _twitterService.OnNewTweet += Twitter_OnNewTweet;
        }

        [Command("RandomElonTweet")]
        [Aliases("ElonTweet", "ret")]
        [Description("Get random Elon's tweet.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.ElonMusk);
            var embed = _twitterEmbedGenerator.Build(tweet);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("RandomSpaceXTweet")]
        [Aliases("SpaceXTweet", "rst")]
        [Description("Get random SpaceX's tweet.")]
        public async Task RandomSpaceXTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.SpaceX);
            var embed = _twitterEmbedGenerator.Build(tweet);

            await ctx.RespondAsync(embed: embed);
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

        private async void Twitter_OnNewTweet(object sender, ITweet tweet)
        {
            var subscriptionType = _twitterService.GetSubscriptionTypeByUserName(tweet.CreatedBy.ScreenName);
            var channels = _subscriptionsService.GetSubscribedChannels(subscriptionType);

            foreach (var channelData in channels)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    var embed = _twitterEmbedGenerator.Build(new CachedTweet(tweet));

                    await channel.SendMessageAsync(embed: embed);
                }
                catch (UnauthorizedException ex)
                {
                    var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                    var guildOwner = guild.Owner;

                    _logger.Error(ex, $"No permissions to send message on channel {channelData.ChannelId}, removing all subscriptions and sending message to {guildOwner.Nickname}.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                    var ownerDm = await guildOwner.CreateDmChannelAsync();
                    var errorEmbed = _twitterEmbedGenerator.BuildUnauthorizedError();

                    await ownerDm.SendMessageAsync(embed: errorEmbed);
                }
                catch (NotFoundException ex)
                {
                    _logger.Error(ex, $"Channel {channelData.ChannelId} not found, removing all subscriptions.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send tweet on the channel with id {channelData.ChannelId}");
                }
            }
        }
    }
}
