using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Subscriptions;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Notifications)]
    public class LaunchNotificationsCommands
    {
        private SubscriptionsService _subscriptionsService;
        private LaunchNotificationsService _launchNotificationsService;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        public LaunchNotificationsCommands(SubscriptionsService subscriptionsService, LaunchNotificationsService launchNotificationsService)
        {
            _subscriptionsService = subscriptionsService;
            _launchNotificationsService = launchNotificationsService;

            _launchNotificationsService.OnLaunchNoification += LaunchNotificationsOnLaunchNoification;
        }

        private async void LaunchNotificationsOnLaunchNoification(object sender, LaunchNotification launchNotification)
        {
            var oldLaunchState = launchNotification.OldLaunchState;
            var launch = launchNotification.NewLaunchState;

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = launch.Links.MissionPatch ?? Constants.SpaceXLogoImage
            };

            var timeLeft = (launch.LaunchDateUtc - DateTime.Now.ToUniversalTime()).Value.TotalMinutes;

            switch (launchNotification.Type)
            {
                case LaunchNotificationType.Reminder:
                {
                    var timeLeftDescription = timeLeft > 60 ? Math.Ceiling(timeLeft / 60) + " hours" : Math.Ceiling(timeLeft) + " minutes";

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"**{timeLeftDescription}** to launch {launch.MissionName}! ");
                    descriptionBuilder.Append($"Type `e!nextlaunch` or `e!getlaunch {launch.FlightNumber.Value}` to get more information.");

                    embed.AddField(":rocket: Launch is upcoming!", descriptionBuilder.ToString());
                    break;
                }

                case LaunchNotificationType.Scrub:
                {
                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"**{launch.MissionName}** launch time has been changed from " +
                                              $"**{oldLaunchState.LaunchDateUtc.Value.ToString("F", CultureInfo.InvariantCulture)}** to " +
                                              $"**{launch.LaunchDateUtc.Value.ToString("F", CultureInfo.InvariantCulture)}**.");

                    descriptionBuilder.Append($"Type `e!nextlaunch` or `e!getlaunch {launch.FlightNumber.Value}` to get more information.");

                    embed.AddField(":warning: Scrub!", descriptionBuilder.ToString());
                    break;
                }

                case LaunchNotificationType.NewTarget:
                {
                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"Good luck **{launchNotification.OldLaunchState.MissionName}**! ");
                    descriptionBuilder.Append($"Next launch will be **{launchNotification.NewLaunchState.MissionName}** at {launchNotification.NewLaunchState.LaunchDateUtc.Value.ToString("F", CultureInfo.InvariantCulture)} UTC. ");

                    descriptionBuilder.Append($"Type `e!nextlaunch` or `e!getlaunch {launch.FlightNumber.Value}` to get more information.");

                    embed.AddField(":rocket: Liftoff!", descriptionBuilder.ToString());
                    break;
                }
            }

            embed.AddField("\u200b", "*Click below reaction to subscribe this flight and be notified on DM 10 minutes before the launch.*");

            var channelIds = _subscriptionsService.GetSubscribedChannels(SubscriptionType.NextLaunch);
            foreach (var channelId in channelIds)
            {
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(channelId);
                    var sentMessage = await channel.SendMessageAsync("", false, embed);

                    await sentMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":regional_indicator_s:"));
                    using (var databaseContext = new DatabaseContext())
                    {
                        databaseContext.MessagesToSubscribe.Add(new MessageToSubscribe(sentMessage.Id.ToString()));
                        databaseContext.SaveChanges();
                    }

                    if (launchNotification.Type == LaunchNotificationType.Reminder && timeLeft < 60 && launch.Links.VideoLink != null)
                    {
                        await channel.SendMessageAsync($"**YouTube stream:** {launch.Links.VideoLink}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send launch notification to the channel with id {channelId}");
                }
            }
        }
    }
}
