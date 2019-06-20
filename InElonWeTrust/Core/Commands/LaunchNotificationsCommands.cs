using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Subscriptions;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Notifications)]
    public class LaunchNotificationsCommands : BaseCommandModule
    {
        private readonly SubscriptionsService _subscriptionsService;
        private readonly LaunchNotificationsService _launchNotificationsService;
        private readonly LaunchNotificationEmbedBuilder _launchNotificationEmbedBuilder;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LaunchNotificationsCommands(SubscriptionsService subscriptionsService, LaunchNotificationsService launchNotificationsService, LaunchNotificationEmbedBuilder launchNotificationEmbedBuilder)
        {
            _subscriptionsService = subscriptionsService;
            _launchNotificationsService = launchNotificationsService;
            _launchNotificationEmbedBuilder = launchNotificationEmbedBuilder;

            _launchNotificationsService.OnLaunchNotification += LaunchNotificationsOnLaunchNotification;
        }

        [Command("FooBar")]
        [HiddenCommand]
        public async Task FooBar(CommandContext ctx)
        {
            // TODO: think about it and create issue
        }

        private async void LaunchNotificationsOnLaunchNotification(object sender, LaunchNotification launchNotification)
        {
            var embed = _launchNotificationEmbedBuilder.Build(launchNotification);
            var channels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.NextLaunch);

            var timeLeft = (launchNotification.NewLaunchState.LaunchDateUtc - DateTime.Now.ToUniversalTime()).Value.TotalMinutes;

            foreach (var channelData in channels)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    var sentMessage = await channel.SendMessageAsync(string.Empty, false, embed);

                    await sentMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":regional_indicator_s:"));
                    using (var databaseContext = new DatabaseContext())
                    {
                        var messageToSubscribe = new MessageToSubscribe(channel.GuildId.ToString(), sentMessage.Id.ToString());

                        databaseContext.MessagesToSubscribe.Add(messageToSubscribe);
                        databaseContext.SaveChanges();
                    }

                    if (launchNotification.Type == LaunchNotificationType.Reminder && timeLeft < 60 && launchNotification.NewLaunchState.Links.VideoLink != null)
                    {
                        await channel.SendMessageAsync($"**YouTube stream:** {launchNotification.NewLaunchState.Links.VideoLink}");
                    }
                }
                catch (UnauthorizedException)
                {
                    var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                    var guildOwner = guild.Owner;

                    _logger.Warn($"No permissions to send message on channel {channelData.ChannelId}, removing all subscriptions and sending message to {guildOwner.Nickname}.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                    var ownerDm = await guildOwner.CreateDmChannelAsync();
                    var errorEmbed = _launchNotificationEmbedBuilder.BuildUnauthorizedError();

                    await ownerDm.SendMessageAsync(embed: errorEmbed);
                }
                catch (NotFoundException)
                {
                    _logger.Warn($"Channel {channelData.ChannelId} not found, removing all subscriptions.");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send launch notification on the channel with id {channelData.ChannelId}");
                }
            }

            _logger.Info($"Launch notifications sent to {channels.Count} channels");
        }
    }
}
