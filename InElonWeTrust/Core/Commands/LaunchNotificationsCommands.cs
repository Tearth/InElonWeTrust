using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":warning:", "Launch notifications", "Commands to manage notifications when launch is incoming")]
    public class LaunchNotificationsCommands
    {
        private SubscriptionsService _subscriptions;

        public LaunchNotificationsCommands()
        {
            _subscriptions = new SubscriptionsService();
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
    }
}
