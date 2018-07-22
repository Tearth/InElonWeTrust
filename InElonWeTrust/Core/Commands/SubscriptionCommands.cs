using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Notifications)]
    public class SubscriptionCommands
    {
        private readonly SubscriptionsService _subscriptionsService;

        public SubscriptionCommands(SubscriptionsService subscriptionsService)
        {
            _subscriptionsService = subscriptionsService;
        }

        [Command("ToggleElonTwitter")]
        [Aliases("TogElonTwitter", "SubscribeElonTwitter", "SubElonTwitter", "tet")]
        [Description("Subscribe Elon Musk Twitter profile (bot will post all new tweets).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleElonTwitterNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Twitter has been subscribed! Now bot will post all newest tweets from " +
                                           "[Elon Musk](https://twitter.com/elonmusk) profile.",
                                           "Twitter subscription has been removed.",
                                           SubscriptionType.ElonTwitter);
        }

        [Command("ToggleSpaceXTwitter")]
        [Aliases("TogSpaceXTwitter", "SubscribeSpaceXTwitter", "SubSpaceXTwitter", "tst")]
        [Description("Subscribe SpaceX Twitter profile (bot will post all new tweets).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleSpaceXTwitterNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Twitter has been subscribed! Now bot will post all newest tweets from " +
                                           "[SpaceX](https://twitter.com/SpaceX) profile.",
                "Twitter subscription has been removed.",
                SubscriptionType.SpaceXTwitter);
        }

        [Command("ToggleFlickr")]
        [Aliases("TogFlickr", "SubscribeFlickr", "SubFlickr", "tf")]
        [Description("Subscribe Flickr profile (bot will post all new photos from SpaceX).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleFlickrNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Flickr has been subscribed! Now bot will post all newest photos from " +
                                           "[SpaceX](https://www.flickr.com/photos/spacex/) profile",
                "Flickr subscribion has been removed.",
                SubscriptionType.Flickr);
        }

        [Command("ToggleLaunches")]
        [Aliases("TogLaunches", "SubscribeLaunches", "SubLaunches", "tl")]
        [Description("Subscribe launch notifications (when next launch is incoming).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleLaunchNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Launch notifications has been subscribed! Now bot will post all newest information " +
                                           "about upcoming launch.",
                "Launch notifications subscription has been removed.",
                SubscriptionType.NextLaunch);
        }

        [Command("ToggleReddit")]
        [Aliases("SubscribeReddit", "SubReddit", "tr")]
        [Description("Subscribe Reddit notifications (when next Reddit is incoming).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleRedditNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Reddit notifications has been subscribed! Now bot will post all newest information " +
                                           "about upcoming Reddit.",
                "Reddit notifications subscription has been removed.",
                SubscriptionType.Reddit);
        }

        [Command("EnableAllNotifications")]
        [Aliases("SubscribeAll", "SubAll", "ean")]
        [Description("Subscribe all notifications.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task EnableAllNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await _subscriptionsService.AddAllSubscriptionsAsync(ctx.Channel.Id);

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", "All notifications has been subscribed.");
            await ctx.RespondAsync("", false, embed);
        }

        [Command("DisableAllNotifications")]
        [Aliases("UnsubscribeAll", "UnsubAll", "dan")]
        [Description("Unsubscribe all notifications.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DisableAllNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await _subscriptionsService.RemoveAllSubscriptionsAsync(ctx.Channel.Id);

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", "All subscriptions has been removed.");
            await ctx.RespondAsync("", false, embed);
        }

        [Command("NotificationsStatus")]
        [Aliases("SubscriptionsStatus", "SubStatus", "ns")]
        [Description("Get information about subscriptions related with this channel.")]
        public async Task NotificationsStatus(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"**Elon Twitter:** {await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.ElonTwitter)}\r\n");
            contentBuilder.Append($"**SpaceX Twitter:** {await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.SpaceXTwitter)}\r\n");
            contentBuilder.Append($"**Flickr:** {await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.Flickr)}\r\n");
            contentBuilder.Append($"**Launches:** {await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.NextLaunch)}\r\n");
            contentBuilder.Append($"**Reddit:** {await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.Reddit)}");

            embed.AddField("Notifications status", contentBuilder.ToString());
            await ctx.RespondAsync("", false, embed);
        }

        private async Task ToggleNotifications(CommandContext ctx, string messageOnAdd, string messageOnRemove, SubscriptionType type)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            if (await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, type))
            {
                await _subscriptionsService.RemoveSubscriptionAsync(ctx.Channel.Id, type);

                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!", messageOnRemove);
            }
            else
            {
                await _subscriptionsService.AddSubscriptionAsync(ctx.Channel.Id, type);

                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!", messageOnAdd);
            }

            await ctx.RespondAsync("", false, embed);
        }
    }
}
