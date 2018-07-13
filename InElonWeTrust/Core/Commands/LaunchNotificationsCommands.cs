using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":warning:", "Launch notifications", "Commands to manage notifications when launch is incoming")]
    public class LaunchNotificationsCommands
    {
        private SubscriptionsService _subscriptions;
        private LaunchNotificationsService _launchNotifications;

        public LaunchNotificationsCommands()
        {
            _subscriptions = new SubscriptionsService();
            _launchNotifications = new LaunchNotificationsService();

            _launchNotifications.OnLaunchNoification += LaunchNotificationsOnOnLaunchNoification;
        }

        [Command("SubscribeLaunchNotifications")]
        [Aliases("SubLaunchNotifies", "SubNotifies", "sln")]
        [Description("Subscribe launch notifications (when next launch is incoming).")]
        public async Task AddLaunchNotificationsChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.AddSubscriptionAsync(ctx.Channel.Id, SubscriptionType.NextLaunch))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!",
                    "Channel has been added to the launch notifications subscription list. Now I will display " +
                    "all news about upcoming launch.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Error!",
                    "Channel is already subscribed.");
            }

            await ctx.RespondAsync("", false, embed);
        }

        [Command("UnsubscribeLaunchNotifications")]
        [Aliases("UnsubLaunchNotifications", "UnsubNotifications", "uln")]
        [Description("Removes launch notifications subscription.")]
        public async Task RemoveLaunchNotificationsChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.RemoveSubscriptionAsync(ctx.Channel.Id, SubscriptionType.NextLaunch))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!",
                    "Channel has been removed the launch notifications subscription list.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Error!",
                    "Channel is already removed.");
            }

            await ctx.RespondAsync("", false, embed);
        }

        [Command("IsLaunchNotificationsSubscribed")]
        [Aliases("LaunchNotificationsSubscribed", "NotificationsSubscribed", "lns")]
        [Description("Checks if the channel is on the launch notifications subscription list.")]
        public async Task IsLaunchNotificationsChannelSubscribed(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.NextLaunch))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Launch notifications subscription status!",
                    "Channel is subscribed.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Launch notifications subscription status!",
                    "Channel is not subscribed.");
            }

            await ctx.RespondAsync("", false, embed);
        }


        private async void LaunchNotificationsOnOnLaunchNoification(object sender, LaunchNotification launchNotification)
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
                    var timeLeftDescription = timeLeft > 60 ? Math.Ceiling(timeLeft / 60) + "hours" : Math.Ceiling(timeLeft) + " minutes";

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
            }

            var channelIds = _subscriptions.GetSubscribedChannels(SubscriptionType.NextLaunch);
            foreach (var channelId in channelIds)
            {
                var channel = await Bot.Client.GetChannelAsync(channelId);
                await channel.SendMessageAsync("", false, embed);

                if (launchNotification.Type == LaunchNotificationType.Reminder && timeLeft < 60 && launch.Links.VideoLink != null)
                {
                    await channel.SendMessageAsync($"**YouTube stream:** {launch.Links.VideoLink}");
                }
            }
        }
    }
}
