using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":newspaper2:", "Subscriptions", "(admins only)")]
    public class SubscriptionCommands
    {
        private SubscriptionsService _subscriptions;

        public SubscriptionCommands()
        {
            _subscriptions = new SubscriptionsService();
        }

        [Command("ToggleTwitter")]
        [Aliases("SubscribeTwitter", "SubTwitter", "tt")]
        [Description("Subscribe SpaceX & Elon Musk Twitter profiles (bot will post all new tweets).")]
        public async Task ToggleTwitterNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Twitter has been subscribed! Now bot will post all newest tweets from " +
                                           "[SpaceX](https://twitter.com/SpaceX)and [Elon Musk](https://twitter.com/elonmusk)" +
                                           "profiles.",
                                           "Twitter subscription has been removed.",
                                           SubscriptionType.Twitter);
        }

        [Command("ToggleFlickr")]
        [Aliases("SubscripbeFlickr", "SubFlickr", "tf")]
        [Description("Subscribe Flickr profile (bot will post all new photos from SpaceX).")]
        public async Task ToggleFlickrNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Flickr has been subscribed! Now bot will post all newest photos from " +
                                           "[SpaceX](https://www.flickr.com/photos/spacex/) profile",
                "Flickr subscribion has been removed.",
                SubscriptionType.Flickr);
        }

        [Command("ToggleLaunchNotifications")]
        [Aliases("SubLaunchNotifications", "SubNotifications", "sln")]
        [Description("Subscribe launch notifications (when next launch is incoming).")]
        public async Task ToggleLaunchNotifications(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ToggleNotifications(ctx, "Launch notifications has been subscribed! Now bot will post all newest information " +
                                           "about upcoming launch.",
                "Launch notifications subscription has been removed.",
                SubscriptionType.NextLaunch);
        }

        [Command("NotificationsStatus")]
        [Aliases("SubscriptionsStatus", "SubStatus", "ns")]
        [Description("Get information about subscriptions related with this channel.")]
        public async Task NotificationsStatus(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"Twitter: {await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.Twitter)}\r\n");
            contentBuilder.Append($"Flickr: {await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.Flickr)}\r\n");
            contentBuilder.Append($"Launches: {await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.NextLaunch)}");

            embed.AddField("Notifications status", contentBuilder.ToString());
            await ctx.RespondAsync("", false, embed);
        }

        private async Task ToggleNotifications(CommandContext ctx, string messageOnAdd, string messageOnRemove, SubscriptionType type)
        {
            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, type))
            {
                await _subscriptions.RemoveSubscriptionAsync(ctx.Channel.Id, type);

                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!", messageOnRemove);
            }
            else
            {
                await _subscriptions.AddSubscriptionAsync(ctx.Channel.Id, type);

                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!", messageOnAdd);
            }

            await ctx.RespondAsync("", false, embed);
        }
    }
}
