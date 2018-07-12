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

        [Command("SubscribeFlickr")]
        [Aliases("SubFlickr", "sf")]
        [Description("Subscribe Flickr profile (bot will post all new photos from SpaceX).")]
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

        [Command("UnsubscribeFlickr")]
        [Aliases("UnsubFlickr", "usf")]
        [Description("Removes Flickr subscription.")]
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

        [Command("IsFlickrSubscribed")]
        [Aliases("FlickrSubscribed", "ifs")]
        [Description("Checks if the channel is on the Flickr subscription list.")]
        public async Task IsFlickrChannelSubscribed(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.IsChannelSubscribed(ctx.Channel.Id, SubscriptionType.Flickr))
            {
                embed.Color = new DiscordColor(Constants.EmbedColor);
                embed.AddField(":rocket: Flickr subscription status!",
                    "Channel is subscribed.");
            }
            else
            {
                embed.Color = new DiscordColor(Constants.EmbedErrorColor);
                embed.AddField(":octagonal_sign: Flickr subscription status!",
                    "Channel is not subscribed.");
            }

            await ctx.RespondAsync("", false, embed);
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
