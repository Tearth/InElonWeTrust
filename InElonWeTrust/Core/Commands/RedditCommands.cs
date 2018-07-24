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
    public class RedditCommands
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

            _redditService.OnNewHotTopic += Reddit_OnNewHotTopic;
        }

        [Command("RandomRedditTopic")]
        [Aliases("RandomReddit", "RandomTopic", "rrt")]
        [Description("Get random Reddit topic from /s/spacex.")]
        public async Task RandomRedditTopic(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var topic = await _redditService.GetRandomTopic();
            var embed = _redditEmbedGenerator.Build(topic);

            await ctx.RespondAsync(embed: embed);
        }

        [HiddenCommand]
        [Command("ReloadRedditCache")]
        [Description("Reload cached Flickr photos in database.")]
        public async Task ReloadRedditCache(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _redditService.ReloadCachedTopicsAsync();
        }

        private async void Reddit_OnNewHotTopic(object sender, RedditChildData e)
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
                catch (UnauthorizedException ex)
                {
                    var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                    var guildOwner = guild.Owner;

                    _logger.Error(ex, $"No permissions to send message on channel {channelData.ChannelId}, removing all subscriptions and sending message to {guildOwner.Nickname}.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                    var ownerDm = await guildOwner.CreateDmChannelAsync();
                    var errorEmbed = _redditEmbedGenerator.BuildUnauthorizedError();

                    await ownerDm.SendMessageAsync(embed: errorEmbed);
                }
                catch (NotFoundException ex)
                {
                    _logger.Error(ex, $"Channel {channelData.ChannelId} not found, removing all subscriptions.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send Reddit topic on the channel with id {channelData.ChannelId}");
                }
            }
        }
    }
}
