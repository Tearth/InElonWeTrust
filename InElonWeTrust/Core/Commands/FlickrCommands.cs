using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.Subscriptions;

namespace InElonWeTrust.Core.Commands
{
    [Commands("SpaceX Flickr")]
    public class FlickrCommands
    {
        private FlickrService _flickr;
        private SubscriptionsService _subscriptions;

        public FlickrCommands()
        {
            _flickr = new FlickrService();
            _subscriptions = new SubscriptionsService();

            _flickr.OnNewFlickrPhoto += Flickr_OnNewFlickrPhoto;
        }

        [Command("subscribeflickr")]
        [Aliases("subflickr", "sf")]
        [Description("Subscribe SpaceX Flickr profile (bot will post all new photos).")]
        public async Task AddFlickrChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.AddSubscriptionAsync(ctx.Channel.Id, SubscriptionType.Flickr))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!",
                    "Channel has been added to the Flickr subscription list. Now I will display " +
                    "all newest images from SpaceX profile.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Error!",
                    "Channel is already subscribed.");
            }

            await ctx.RespondAsync("", false, embed);
        }

        [Command("unsubscribeflickr")]
        [Aliases("unsubflickr", "usf")]
        [Description("Removes SpaceX Flickr subscription.")]
        public async Task RemoveFlickrChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.RemoveSubscriptionAsync(ctx.Channel.Id, SubscriptionType.Flickr))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Success!",
                    "Channel has been removed the Flickr subscription list.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Error!",
                    "Channel is already removed.");
            }

            await ctx.RespondAsync("", false, embed);
        }

        [Command("randomflickrphoto")]
        [Aliases("randomfp", "rfp")]
        [Description("Get random photo from SpaceX Flickr profile.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            var photo = await _flickr.GetRandomPhotoAsync();
            await DisplayPhoto(ctx.Channel, photo);
        }

        [HiddenCommand]
        [Command("reloadcachedflickrphotos")]
        [Aliases("reloadcfp", "rcfp")]
        [Description("Reload cached Flickr photos in database.")]
        public async Task ReloadCachedTweets(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Reload cached Flickr photos starts");
            await _flickr.ReloadCachedPhotosAsync(false);
            await ctx.Channel.SendMessageAsync("Reload cached Flickr photos finished");
        }

        private async void Flickr_OnNewFlickrPhoto(object sender, CachedFlickrPhoto e)
        {
            var channels = _subscriptions.GetSubscribedChannels(SubscriptionType.Flickr);
            foreach (var channelId in channels)
            {
                var channel = await Bot.Client.GetChannelAsync(channelId);
                await DisplayPhoto(channel, e);
            }
        }

        private async Task DisplayPhoto(DiscordChannel channel, CachedFlickrPhoto photo)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ImageUrl = photo.Source
            };

            embed.AddField($"{photo.Title} ({photo.UploadDate})", $"https://www.flickr.com/photos/spacex/{photo.Id}");
            await channel.SendMessageAsync("", false, embed);
        }
    }
}
