using System;
using System.Globalization;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.LaunchNotifications;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class LaunchNotificationEmbedBuilder
    {
        public DiscordEmbedBuilder Build(LaunchNotification launchNotification)
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

                        embed.AddField(":rocket: Launch is coming!", descriptionBuilder.ToString());
                        break;
                    }

                case LaunchNotificationType.Scrub:
                    {
                        var descriptionBuilder = new StringBuilder();
                        descriptionBuilder.Append($"**{launch.MissionName}** launch time has been changed from " +
                                                  $"**{oldLaunchState.LaunchDateUtc.Value.ToString("F", CultureInfo.InvariantCulture)} UTC** to " +
                                                  $"**{launch.LaunchDateUtc.Value.ToString("F", CultureInfo.InvariantCulture)} UTC**" +
                                                  $"{(launch.TentativeMaxPrecision.HasValue ? $" ({launch.TentativeMaxPrecision.Value.ToString().ToLower()} precision)" : string.Empty)}. ");

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

            return embed;
        }


        public DiscordEmbedBuilder BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has no enough permissions to post launch notification. Check it and subscribe launch notifications again.");

            return embed;
        }
    }
}
