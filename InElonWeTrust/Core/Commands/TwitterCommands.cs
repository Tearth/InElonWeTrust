using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Services.Twitter;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Media)]
    public class TwitterCommands : BaseCommandModule
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

            _twitterService.OnNewTweets += TwitterOnNewTweetsAsync;
        }

        [Command("RandomElonTweet"), Aliases("ElonTweet")]
        [Description("Get a random Elon's tweet.")]
        public async Task RandomElonTweetAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.ElonMusk);
            var embed = _twitterEmbedGenerator.Build(tweet);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("RandomSpaceXTweet"), Aliases("SpaceXTweet")]
        [Description("Get a random SpaceX's tweet.")]
        public async Task RandomSpaceXTweetAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.SpaceX);
            var embed = _twitterEmbedGenerator.Build(tweet);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("RandomSpaceXFleetTweet"), Aliases("SpaceXFleetTweet")]
        [Description("Get a random SpaceXFleet's tweet.")]
        public async Task RandomSpaceXFleetTweetAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var tweet = await _twitterService.GetRandomTweetAsync(TwitterUserType.SpaceXFleet);
            var embed = _twitterEmbedGenerator.Build(tweet);

            await ctx.RespondAsync(embed: embed);
        }

        [RequireOwner]
        [Hidden, Command("ReloadTwitterCache")]
        [Description("Reload cached tweets in the database.")]
        public async Task ReloadTwitterCacheAsync(CommandContext ctx)
        {
            await _twitterService.ReloadCachedTweetsAsync(false);
        }

        private async void TwitterOnNewTweetsAsync(object sender, List<CachedTweet> tweets)
        {
            var user = (KeyValuePair<TwitterUserType, string>)sender;
            var subscriptionType = _twitterService.GetSubscriptionTypeByUserName(user.Value);
            var channels = _subscriptionsService.GetSubscribedChannels(subscriptionType);
            var embedsToSend = tweets.Select(p => _twitterEmbedGenerator.Build(p)).ToList();

            foreach (var channelData in channels)
            {
                try
                {
                    await SendTweetsToChannel(channelData, embedsToSend);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "General error occurred when trying to send Twitter notification");
                }
            }

            _logger.Info($"{tweets.Count} Twitter notifications sent to {channels.Count} channels");
        }

        private async Task SendTweetsToChannel(SubscribedChannel channelData, List<DiscordEmbed> tweetEmbeds)
        {
            try
            {
                foreach (var embed in tweetEmbeds)
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    await channel.SendMessageAsync(embed: embed);
                }
            }
            catch (UnauthorizedException ex)
            {
                var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                var guildOwner = guild.Owner;

                _logger.Warn($"No permissions to send message on channel [{channelData.ChannelId}], " +
                             $"removing all subscriptions and sending message to {guildOwner.Username} [{guildOwner.Id}]");
                _logger.Warn($"JSON: {ex.JsonMessage}");

                await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(
                    ulong.Parse(channelData.ChannelId));

                var ownerDm = await guildOwner.CreateDmChannelAsync();
                var errorEmbed = _twitterEmbedGenerator.BuildUnauthorizedError();
                await ownerDm.SendMessageAsync(embed: errorEmbed);
            }
            catch (NotFoundException ex)
            {
                _logger.Warn($"Channel [{channelData.ChannelId}] not found, removing all subscriptions");
                _logger.Warn($"JSON: {ex.JsonMessage}");

                await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(
                    ulong.Parse(channelData.ChannelId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Can't send tweet on the channel with id [{channelData.ChannelId}]");
            }
        }
    }
}
