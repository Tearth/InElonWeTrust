using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Notifications)]
    public class SubscriptionCommands : BaseCommandModule
    {
        private readonly SubscriptionsService _subscriptionsService;
        private readonly SubscriptionEmbedGenerator _subscriptionEmbedGenerator;

        public SubscriptionCommands(SubscriptionsService subscriptionsService, SubscriptionEmbedGenerator subscriptionEmbedGenerator)
        {
            _subscriptionsService = subscriptionsService;
            _subscriptionEmbedGenerator = subscriptionEmbedGenerator;

            Bot.Client.GuildDeleted += Client_GuildDeletedAsync;
            Bot.Client.ChannelDeleted += Client_ChannelDeletedAsync;
        }

        [Command("ToggleElonTwitter"), Aliases("ToggleElonTwitterNotifications")]
        [Description("Toggle Elon Musk Twitter profile (the bot will post all new tweets on this channel).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleElonTwitterNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotificationsAsync(ctx, SubscriptionType.ElonTwitter);
        }

        [Command("ToggleSpaceXTwitter"), Aliases("ToggleSpaceXTwitterNotifications")]
        [Description("Toggle SpaceX Twitter profile (the bot will post all new tweets).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleSpaceXTwitterNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotificationsAsync(ctx, SubscriptionType.SpaceXTwitter);
        }

        [Command("ToggleSpaceXFleetTwitter"), Aliases("ToggleSpaceXFleetTwitterNotifications")]
        [Description("Toggle SpaceX Twitter profile (the bot will post all new tweets on this channel).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleSpaceXFleetTwitterNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotificationsAsync(ctx, SubscriptionType.SpaceXFleetTwitter);
        }

        [Command("ToggleFlickr"), Aliases("ToggleFlickrNotifications")]
        [Description("Toggle Flickr profile (the bot will post all new photos from SpaceX profile on this channel).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleFlickrNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotificationsAsync(ctx, SubscriptionType.Flickr);
        }

        [Command("ToggleLaunches"), Aliases("ToggleLaunchNotifications")]
        [Description("Toggle launch notifications (the bot will post notification on this channel when the next launch will be coming).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleLaunchNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotificationsAsync(ctx, SubscriptionType.NextLaunch);
        }

        [Command("ToggleReddit"), Aliases("ToggleRedditNotifications")]
        [Description("Toggle Reddit notifications (the bot will post all hottest Reddit topics on this channel).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ToggleRedditNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotificationsAsync(ctx, SubscriptionType.Reddit);
        }

        [Command("EnableAllNotifications"), Aliases("EnableNotifications", "EnableNotification")]
        [Description("Enable all notifications (Twitter, Reddit, Flickr, launches) on this channel.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task EnableAllNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await _subscriptionsService.AddAllSubscriptionsAsync(ctx.Guild.Id, ctx.Channel.Id);

            var embed = _subscriptionEmbedGenerator.BuildMessageOnAddAll();
            await ctx.RespondAsync(string.Empty, false, embed);
        }

        [Command("DisableAllNotifications"), Aliases("DisableNotifications", "DisableNotification")]
        [Description("Disable all notifications (Twitter, Reddit, Flickr, launches) on this channel.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DisableAllNotificationsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ctx.Channel.Id);

            var embed = _subscriptionEmbedGenerator.BuildMessageOnRemoveAll();
            await ctx.RespondAsync(string.Empty, false, embed);
        }

        [Command("NotificationStatus"), Aliases("Notifications", "NotificationsStatus")]
        [Description("Get an information about subscriptions related to this channel.")]
        public async Task NotificationStatusAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var status = await _subscriptionsService.GetSubscriptionStatusForChannel(ctx.Channel.Id);
            var embed = _subscriptionEmbedGenerator.BuildStatus(status);

            await ctx.RespondAsync(embed: embed);
        }

        private async Task ToggleNotificationsAsync(CommandContext ctx,  SubscriptionType type)
        {
            DiscordEmbed embed;
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

        private async Task Client_GuildDeletedAsync(DiscordClient client, GuildDeleteEventArgs e)
        {
            await _subscriptionsService.RemoveAllSubscriptionsFromGuildAsync(e.Guild.Id);
        }

        private async Task Client_ChannelDeletedAsync(DiscordClient client, ChannelDeleteEventArgs e)
        {
            await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(e.Channel.Id);
        }
    }
}
