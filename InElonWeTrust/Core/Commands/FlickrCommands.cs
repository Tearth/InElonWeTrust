using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Settings;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":frame_photo:", "Media", "Commands related with Twitter, Flickr and Reddit")]
    public class FlickrCommands
    {
        private FlickrService _flickrService;
        private SubscriptionsService _subscriptionsService;

        public FlickrCommands(FlickrService flickrService, SubscriptionsService subscriptionsService)
        {
            _flickrService = flickrService;
            _subscriptionsService = subscriptionsService;

            _flickrService.OnNewFlickrPhoto += FlickrServiceOnNewFlickrServicePhoto;
        }

        [Command("RandomFlickrPhoto")]
        [Aliases("FlickrPhoto", "rfp")]
        [Description("Get random photo from SpaceX Flickr profile.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var photo = await _flickrService.GetRandomPhotoAsync();
            await DisplayPhoto(ctx.Channel, photo);
        }

        [HiddenCommand]
        [Command("ReloadFlickrCache")]
        [Description("Reload cached Flickr photos in database.")]
        public async Task ReloadFlickrCache(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _flickrService.ReloadCachedPhotosAsync(false);
        }

        private async void FlickrServiceOnNewFlickrServicePhoto(object sender, CachedFlickrPhoto e)
        {
            var channels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.Flickr);
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
