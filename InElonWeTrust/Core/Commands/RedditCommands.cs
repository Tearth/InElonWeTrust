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
using InElonWeTrust.Core.Services.Reddit;
using InElonWeTrust.Core.Services.Subscriptions;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Media)]
    public class RedditCommands : BaseCommandModule
    {
        private readonly RedditService _redditService;
        private readonly SubscriptionsService _subscriptionsService;
        private readonly RedditEmbedGenerator _redditEmbedGenerator;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RedditCommands(RedditService redditService, SubscriptionsService subscriptionsService, RedditEmbedGenerator redditEmbedGenerator)
        {
            _redditService = redditService;
            _subscriptionsService = subscriptionsService;
            _redditEmbedGenerator = redditEmbedGenerator;

            _redditService.OnNewHotTopic += Reddit_OnNewHotTopicAsync;
        }

        [Hidden, Command("RandomRedditTopic"), Aliases("RandomReddit", "RandomTopic")]
        [Description("Get a random Reddit topic from /r/spacex.")]
        public async Task RandomRedditTopicAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var topic = await _redditService.GetRandomTopicAsync();
            var embed = _redditEmbedGenerator.Build(topic);

            await ctx.RespondAsync(embed: embed);
        }

        [RequireOwner]
        [Hidden, Command("ReloadRedditCache")]
        [Description("Reload cached Reddit topics in the database.")]
        public async Task ReloadRedditCacheAsync(CommandContext ctx)
        {
            await _redditService.ReloadCachedTopicsAsync();
        }

        private async void Reddit_OnNewHotTopicAsync(object sender, List<RedditChildData> topics)
        {
            var channels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.Reddit);
            var embedsToSend = topics.Select(p => _redditEmbedGenerator.Build(p)).ToList();

            foreach (var channelData in channels)
            {
                try
                {
                    await SendTopicToChannel(channelData, embedsToSend);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "General error occurred when trying to send Reddit notification");
                }
            }

            _logger.Info($"{topics.Count} Reddit notifications sent to {channels.Count} channels");
        }

        private async Task SendTopicToChannel(SubscribedChannel channelData, List<DiscordEmbed> topicEmbeds)
        {
            try
            {
                foreach (var embed in topicEmbeds)
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    await channel.SendMessageAsync(embed: embed);
                }
            }
            catch (UnauthorizedException ex)
            {
                var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                var guildOwner = guild.Owner;

                _logger.Warn($"No permissions to send message to channel [{channelData.ChannelId}], " +
                             $"removing all subscriptions and sending message to {guildOwner.Username} [{guildOwner.Id}]");
                _logger.Warn($"JSON: {ex.JsonMessage}");

                await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                var ownerDm = await guildOwner.CreateDmChannelAsync();
                var errorEmbed = _redditEmbedGenerator.BuildUnauthorizedError();
                await ownerDm.SendMessageAsync(embed: errorEmbed);
            }
            catch (NotFoundException ex)
            {
                _logger.Warn($"Channel [{channelData.ChannelId}] not found, removing all subscriptions");
                _logger.Warn($"JSON: {ex.JsonMessage}");

                await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Can't send Reddit topic to the channel with id [{channelData.ChannelId}]");
            }
        }
    }
}
