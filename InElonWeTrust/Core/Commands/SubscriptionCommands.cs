using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Notifications)]
    public class SubscriptionCommands
    {
        private readonly SubscriptionsService _subscriptionsService;
        private readonly SubscriptionEmbedGenerator _subscriptionEmbedGenerator;

        public SubscriptionCommands(SubscriptionsService subscriptionsService, SubscriptionEmbedGenerator subscriptionEmbedGenerator)
        {
            _subscriptionsService = subscriptionsService;
            _subscriptionEmbedGenerator = subscriptionEmbedGenerator;

            Bot.Client.GuildDeleted += Client_GuildDeleted;
            Bot.Client.ChannelDeleted += Client_ChannelDeleted;
        }

        [Command("ToggleElonTwitter")]
        [Aliases("TogElonTwitter", "SubscribeElonTwitter", "SubElonTwitter", "tet")]
        [Description("Subscribe Elon Musk Twitter profile (bot will post all new tweets).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleElonTwitterNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, SubscriptionType.ElonTwitter);
        }

        [Command("ToggleSpaceXTwitter")]
        [Aliases("TogSpaceXTwitter", "SubscribeSpaceXTwitter", "SubSpaceXTwitter", "tst")]
        [Description("Subscribe SpaceX Twitter profile (bot will post all new tweets).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleSpaceXTwitterNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, SubscriptionType.SpaceXTwitter);
        }

        [Command("ToggleFlickr")]
        [Aliases("TogFlickr", "SubscribeFlickr", "SubFlickr", "tf")]
        [Description("Subscribe Flickr profile (bot will post all new photos from SpaceX).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleFlickrNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, SubscriptionType.Flickr);
        }

        [Command("ToggleLaunches")]
        [Aliases("TogLaunches", "SubscribeLaunches", "SubLaunches", "tl")]
        [Description("Subscribe launch notifications (when next launch is incoming).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleLaunchNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, SubscriptionType.NextLaunch);
        }

        [Command("ToggleReddit")]
        [Aliases("SubscribeReddit", "SubReddit", "tr")]
        [Description("Subscribe Reddit notifications (when next Reddit is incoming).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleRedditNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, SubscriptionType.Reddit);
        }

        [Command("EnableAllNotifications")]
        [Aliases("SubscribeAll", "SubAll", "ean")]
        [Description("Subscribe all notifications.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task EnableAllNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await _subscriptionsService.AddAllSubscriptionsAsync(ctx.Guild.Id, ctx.Channel.Id);

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
            await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ctx.Channel.Id);

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

            var status = await _subscriptionsService.GetSubscriptionStatusForChannel(ctx.Channel.Id);
            var embed = _subscriptionEmbedGenerator.BuildStatus(status);

            await ctx.RespondAsync(embed: embed);
        }

        private async Task ToggleNotifications(CommandContext ctx,  SubscriptionType type)
        {
            DiscordEmbedBuilder embed;
            if (await _subscriptionsService.IsChannelSubscribed(ctx.Channel.Id, type))
            {
                await _subscriptionsService.RemoveSubscriptionAsync(ctx.Channel.Id, type);
                embed = _subscriptionEmbedGenerator.BuildMessageOnRemove(type);
            }
            else
            {
                await _subscriptionsService.AddSubscriptionAsync(ctx.Guild.Id, ctx.Channel.Id, type);
                embed = _subscriptionEmbedGenerator.BuildMessageOnAdd(type);
            }

            await ctx.RespondAsync(embed: embed);
        }

        private async Task Client_GuildDeleted(GuildDeleteEventArgs e)
        {
            await _subscriptionsService.RemoveAllSubscriptionsFromGuildAsync(e.Guild.Id);
        }

        private async Task Client_ChannelDeleted(ChannelDeleteEventArgs e)
        {
            await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(e.Channel.Id);
        }
    }
}
