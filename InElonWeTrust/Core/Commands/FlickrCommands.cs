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
    [Commands(":frame_photo:", "Flickr", "Commands related with [SpaceX Flickr Profile](https://www.flickr.com/photos/spacex/)")]
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

        [Command("RandomFlickrPhoto")]
        [Aliases("FlickrPhoto", "rfp")]
        [Description("Get random photo from SpaceX Flickr profile.")]
        public async Task RandomElonTweet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var photo = await _flickr.GetRandomPhotoAsync();
            await DisplayPhoto(ctx.Channel, photo);
        }

        [HiddenCommand]
        [Command("ReloadCachedFlickrPhotos")]
        [Description("Reload cached Flickr photos in database.")]
        public async Task ReloadCachedTweets(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _flickr.ReloadCachedPhotosAsync(false);
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
