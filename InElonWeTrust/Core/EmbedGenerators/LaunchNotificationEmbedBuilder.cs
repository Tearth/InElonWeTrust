﻿using System;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Formatters;
using InElonWeTrust.Core.Services.LaunchNotifications;
using Oddity.Models.Launches;

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
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = launch.Links.Patch.Large ?? Constants.SpaceXLogoImage
                }
            };

            var launchTme = launch.DateUtc ?? DateTime.MinValue;
            var now = DateTime.Now.ToUniversalTime();
            var timeLeft = (launchTme - now).TotalMinutes;

            switch (launchNotification.Type)
            {
                case LaunchNotificationType.Reminder:
                {
                    var timeLeftDescription = timeLeft > 60 ? Math.Ceiling(timeLeft / 60) + " hours" : Math.Ceiling(timeLeft) + " minutes";

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"**{timeLeftDescription}** to launch **{launch.Name}**! ");
                    descriptionBuilder.Append($"Type `e!NextLaunch` to get more information.");

                    embed.Title = ":rocket: Launch is coming!";
                    embed.Description = descriptionBuilder.ToString();
                    break;
                }

                case LaunchNotificationType.Scrub:
                {
                    var oldLaunchDate = DateFormatter.GetDateStringWithPrecision(
                        oldLaunchState.DateUtc ?? DateTime.MinValue,
                        oldLaunchState.DatePrecision ?? DatePrecision.Year,
                        true, true, true);

                    var newLaunchDate = DateFormatter.GetDateStringWithPrecision(
                        launch.DateUtc ?? DateTime.MinValue,
                        launch.DatePrecision ?? DatePrecision.Year,
                        true, true, true);

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"**{launch.Name}** launch time has been changed from ");
                    descriptionBuilder.Append($"**{oldLaunchDate}** to ");
                    descriptionBuilder.Append($"**{newLaunchDate}**. ");

                    descriptionBuilder.Append($"Type `e!NextLaunch` to get more information.");

                    embed.Title = ":warning: Scrub!";
                    embed.Description = descriptionBuilder.ToString();
                    break;
                }

                case LaunchNotificationType.NewTarget:
                {
                    var nextLaunchDate = DateFormatter.GetDateStringWithPrecision(
                        launchNotification.NewLaunchState.DateUtc ?? DateTime.MinValue,
                        launchNotification.NewLaunchState.DatePrecision ?? DatePrecision.Year,
                        true, true, true);

                    var descriptionBuilder = new StringBuilder();
                    descriptionBuilder.Append($"The next launch will be **{launchNotification.NewLaunchState.Name}** on **{nextLaunchDate}**. ");
                    descriptionBuilder.Append($"Type `e!NextLaunch` to get more information.");

                    embed.Title = ":rocket: New target!";
                    embed.Description = descriptionBuilder.ToString();
                    break;
                }
            }

            embed.AddField("\u200b", "*Click the reaction below to subscribe this flight and be notified on DM 10 minutes before the launch.*");
            return embed;
        }

        public DiscordEmbed BuildUnauthorizedError()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":octagonal_sign: Oops!",
                Description = "It seems that bot has not enough permissions to post launch notification. Check it and subscribe launch notifications again.",
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };
        }
    }
}
