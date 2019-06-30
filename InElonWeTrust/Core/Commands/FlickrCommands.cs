using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Settings;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Media)]
    public class FlickrCommands : BaseCommandModule
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

            _flickrService.OnNewFlickrPhoto += FlickrServiceOnNewFlickrServicePhotoAsync;
        }

        [Command("RandomFlickrPhoto")]
        [Aliases("FlickrPhoto", "Flickr", "rfp")]
        [Description("Get a random photo from the SpaceX Flickr profile.")]
        public async Task RandomFlickrPhotoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var photo = await _flickrService.GetRandomPhotoAsync();
            var embed = _flickrEmbedGenerator.Build(photo);

            await ctx.RespondAsync(string.Empty, false, embed);
        }

        [Hidden]
        [Command("ReloadFlickrCache")]
        [Description("Reload cached Flickr photos in the database.")]
        public async Task ReloadFlickrCacheAsync(CommandContext ctx)
        {
            if (ctx.User.Id != SettingsLoader.Data.OwnerId)
            {
                return;
            }

            await _flickrService.ReloadCachedPhotosAsync();
        }

        private async void FlickrServiceOnNewFlickrServicePhotoAsync(object sender, List<CachedFlickrPhoto> e)
        {
            var subscribedChannels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.Flickr);
            foreach (var channelData in subscribedChannels)
            {
                try
                {
                    foreach (var photo in e)
                    {
                        var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                        var embed = _flickrEmbedGenerator.Build(photo);

                        await channel.SendMessageAsync(string.Empty, false, embed);
                    }
                }
                catch (UnauthorizedException)
                {
                    var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                    var guildOwner = guild.Owner;

                    _logger.Warn($"No permissions to send message to channel {channelData.ChannelId}, removing all subscriptions and sending message to {guildOwner.Nickname}");

                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                    var ownerDm = await guildOwner.CreateDmChannelAsync();
                    var errorEmbed = _flickrEmbedGenerator.BuildUnauthorizedError();
                    await ownerDm.SendMessageAsync(embed: errorEmbed);
                }
                catch (NotFoundException)
                {
                    _logger.Warn($"Channel {channelData.ChannelId} not found, removing all subscriptions");
                    await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Can't send Flickr photo to the channel with id {channelData.ChannelId}");
                }
            }

            _logger.Info($"Flickr notifications sent to {subscribedChannels.Count} channels");
        }
    }
}
