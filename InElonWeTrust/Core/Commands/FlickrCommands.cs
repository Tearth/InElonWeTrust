using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Settings;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Media)]
    public class FlickrCommands
    {
        private readonly FlickrService _flickrService;
        private readonly SubscriptionsService _subscriptionsService;
        private readonly FlickrEmbedGenerator _flickrEmbedGenerator;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FlickrCommands(FlickrService flickrService, SubscriptionsService subscriptionsService, FlickrEmbedGenerator flickrEmbedGenerator)
        {
            _flickrService = flickrService;
            _subscriptionsService = subscriptionsService;
            _flickrEmbedGenerator = flickrEmbedGenerator;

            _flickrService.OnNewFlickrPhoto += FlickrServiceOnNewFlickrServicePhoto;
        }

        [Command("RandomFlickrPhoto")]
        [Aliases("FlickrPhoto", "rfp")]
        [Description("Get random photo from SpaceX Flickr profile.")]
        public async Task RandomFlickrPhoto(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var photo = await _flickrService.GetRandomPhotoAsync();
            var embed = _flickrEmbedGenerator.Build(photo);

            await ctx.RespondAsync("", false, embed);
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
                try
                {
                    var channel = await Bot.Client.GetChannelAsync(channelId);
                    var embed = _flickrEmbedGenerator.Build(e);

                    await channel.SendMessageAsync("", false, embed);
                }
                catch (UnauthorizedException ex)
                {

                }
                catch (NotFoundException ex)
                {

                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send flickr photo to the channel with id {channelId}");
                }
            }
        }
    }
}
