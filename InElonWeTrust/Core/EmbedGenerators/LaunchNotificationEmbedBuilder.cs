using System;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.LaunchNotifications;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class LaunchNotificationEmbedBuilder
    {
        public DiscordEmbed Build(LaunchNotification launchNotification)
        {
            var oldLaunchState = launchNotification.OldLaunchState;
            var launch = launchNotification.NewLaunchState;

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = launch.Links.MissionPatch ?? Constants.SpaceXLogoImage
            };

            var launchTme = launch.LaunchDateUtc ?? DateTime.MinValue;
            var now = DateTime.Now.ToUniversalTime();
            var timeLeft = (launchTme - now).TotalMinutes;

            switch (launchNotification.Type)
            {
                case LaunchNotificationType.Reminder:
                {
                    var timeLeftDescription = timeLeft > 60 ? Math.Ceiling(timeLeft / 60) + " hours" : Math.Ceiling(timeLeft) + " minutes";

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"**{timeLeftDescription}** to launch {launch.MissionName}! ");
                    descriptionBuilder.Append($"Type `e!nextlaunch` or `e!getlaunch {launch.FlightNumber ?? 0}` to get more information.");

                    embed.AddField(":rocket: Launch is coming!", descriptionBuilder.ToString());
                    break;
                }

                case LaunchNotificationType.Scrub:
                {
                    var oldLaunchDate = DateFormatter.GetDateStringWithPrecision(
                        oldLaunchState.LaunchDateUtc ?? DateTime.MinValue,
                        oldLaunchState.TentativeMaxPrecision ?? TentativeMaxPrecision.Year,
                        true, true, true);

                    var newLaunchDate = DateFormatter.GetDateStringWithPrecision(
                        launch.LaunchDateUtc ?? DateTime.MinValue,
                        launch.TentativeMaxPrecision ?? TentativeMaxPrecision.Year,
                        true, true, true);

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"**{launch.MissionName}** launch time has been changed from ");
                    descriptionBuilder.Append($"**{oldLaunchDate}** to ");
                    descriptionBuilder.Append($"**{newLaunchDate}**. ");

                    descriptionBuilder.Append($"Type `e!nextlaunch` or `e!getlaunch {launch.FlightNumber ?? 0}` to get more information.");

                    embed.AddField(":warning: Scrub!", descriptionBuilder.ToString());
                    break;
                }

                case LaunchNotificationType.NewTarget:
                {
                    var nextLaunchDate = DateFormatter.GetDateStringWithPrecision(
                        launchNotification.NewLaunchState.LaunchDateUtc ?? DateTime.MinValue,
                        launchNotification.NewLaunchState.TentativeMaxPrecision ?? TentativeMaxPrecision.Year,
                        true, true, true);

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"Good luck **{launchNotification.OldLaunchState.MissionName}**! ");
                    descriptionBuilder.Append($"Next launch will be **{launchNotification.NewLaunchState.MissionName}** at **{nextLaunchDate}**. ");
                    descriptionBuilder.Append($"Type `e!nextlaunch` or `e!getlaunch {launch.FlightNumber ?? 0}` to get more information.");

                    embed.AddField(":rocket: Liftoff!", descriptionBuilder.ToString());
                    break;
                }
            }

            embed.AddField("\u200b", "*Click below reaction to subscribe this flight and be notified on DM 10 minutes before the launch.*");

            return embed;
        }


        public DiscordEmbed BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has not enough permissions to post launch notification. Check it and subscribe launch notifications again.");

            return embed;
        }
    }
}
