using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Subscriptions;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Notifications)]
    public class LaunchNotificationsCommands : BaseCommandModule
    {
        private readonly LaunchNotificationsService _launchNotificationsService;
        private readonly SubscriptionsService _subscriptionsService;
        private readonly LaunchNotificationEmbedBuilder _launchNotificationEmbedBuilder;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LaunchNotificationsCommands(SubscriptionsService subscriptionsService, LaunchNotificationsService launchNotificationsService, LaunchNotificationEmbedBuilder launchNotificationEmbedBuilder)
        {
            _launchNotificationsService = launchNotificationsService;
            _subscriptionsService = subscriptionsService;
            _launchNotificationEmbedBuilder = launchNotificationEmbedBuilder;

            launchNotificationsService.OnLaunchNotification += LaunchNotificationsOnLaunchNotificationAsync;
        }

        [Hidden, Command("__LaunchNotificationsCommands__Hidden")]
        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task HiddenCommandAsync(CommandContext ctx)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Fake command necessary for DSharpPlus (without it, constructor won't be called).
        }

        private async void LaunchNotificationsOnLaunchNotificationAsync(object sender, LaunchNotification launchNotification)
        {
            var embed = _launchNotificationEmbedBuilder.Build(launchNotification);
            var channels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.NextLaunch);

            var launchTime = launchNotification.NewLaunchState.LaunchDateUtc ?? DateTime.MinValue;
            if (launchTime == DateTime.MinValue)
            {
                return;
            }

            var timeLeft = (launchTime - DateTime.Now.ToUniversalTime()).TotalMinutes;
            foreach (var channelData in channels)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    var sentMessage = await channel.SendMessageAsync(string.Empty, false, embed);

                    await sentMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":regional_indicator_s:"));
                    await _launchNotificationsService.AddMessageToSubscribe(channel, sentMessage);

                    if (launchNotification.Type == LaunchNotificationType.Reminder && timeLeft < 60 && launchNotification.NewLaunchState.Links.VideoLink != null)
                    {
                        await channel.SendMessageAsync($"**YouTube stream:** {launchNotification.NewLaunchState.Links.VideoLink}");
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
                    var errorEmbed = _launchNotificationEmbedBuilder.BuildUnauthorizedError();

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
                    _logger.Error(ex, $"Can't send launch notification to the channel with id [{channelData.ChannelId}]");
                }
            }

            _logger.Info($"Launch notifications sent to {channels.Count} channels");
        }
    }
}
