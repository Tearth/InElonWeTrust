using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Reddit;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Settings;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Media)]
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

        [Command("RandomRedditTopic")]
        [Aliases("RandomReddit", "RandomTopic", "Reddit", "rrt")]
        [Description("Get a random Reddit topic from /r/spacex.")]
        public async Task RandomRedditTopicAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var topic = await _redditService.GetRandomTopicAsync();
            var embed = _redditEmbedGenerator.Build(topic);

            await ctx.RespondAsync(embed: embed);
        }

        [HiddenCommand]
        [Command("ReloadRedditCache")]
        [Description("Reload cached Reddit topics in the database.")]
        public async Task ReloadRedditCacheAsync(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _redditService.ReloadCachedTopicsAsync();
        }

        private async void Reddit_OnNewHotTopicAsync(object sender, RedditChildData e)
        {
            var channels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.Reddit);
            foreach (var channelData in channels)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    var embed = _redditEmbedGenerator.Build(e);

                    await channel.SendMessageAsync(embed: embed);
                }
                catch (UnauthorizedException)
                {
                    var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                    var guildOwner = guild.Owner;

                    _logger.Warn($"No permissions to send message to channel {channelData.ChannelId}, removing all subscriptions and sending message to {guildOwner.Nickname}.");

                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                    var ownerDm = await guildOwner.CreateDmChannelAsync();
                    var errorEmbed = _redditEmbedGenerator.BuildUnauthorizedError();
                    await ownerDm.SendMessageAsync(embed: errorEmbed);
                }
                catch (NotFoundException)
                {
                    _logger.Warn($"Channel {channelData.ChannelId} not found, removing all subscriptions.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send Reddit topic to the channel with id {channelData.ChannelId}");
                }
            }

            _logger.Info($"Reddit notifications sent to {channels.Count} channels");
        }
    }
}
