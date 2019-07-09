using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.Subscriptions;
using NLog;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Media)]
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

        [Command("RandomFlickrPhoto"), Aliases("RandomPhoto", "FlickrPhoto")]
        [Description("Get a random photo from the SpaceX Flickr profile.")]
        public async Task RandomFlickrPhotoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var photo = await _flickrService.GetRandomPhotoAsync();
            var embed = _flickrEmbedGenerator.Build(photo);

            await ctx.RespondAsync(string.Empty, false, embed);
        }

        [RequireOwner]
        [Hidden, Command("ReloadFlickrCache")]
        [Description("Reload cached Flickr photos in the database.")]
        public async Task ReloadFlickrCacheAsync(CommandContext ctx)
        {
            await _flickrService.ReloadCachedPhotosAsync();
        }

        private async void FlickrServiceOnNewFlickrServicePhotoAsync(object sender, List<CachedFlickrPhoto> photos)
        {
            var subscribedChannels = _subscriptionsService.GetSubscribedChannels(SubscriptionType.Flickr);
            var embedsToSend = photos.Select(p => _flickrEmbedGenerator.Build(p)).ToList();

            var stopwatch = Stopwatch.StartNew();
            foreach (var channelData in subscribedChannels)
            {
                try
                {
                    await SendPhotosToChannel(channelData, embedsToSend);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "General error occurred when trying to send Flickr notification");
                }
            }

            _logger.Info($"{photos.Count} Flickr notifications sent to {subscribedChannels.Count} channels " +
                         $"in {stopwatch.Elapsed.TotalSeconds:0.0} seconds");
        }

        private async Task SendPhotosToChannel(SubscribedChannel channelData, List<DiscordEmbed> photoEmbeds)
        {
            try
            {
                foreach (var embed in photoEmbeds)
                {
                    var channel = await Bot.Client.GetChannelAsync(ulong.Parse(channelData.ChannelId));
                    await channel.SendMessageAsync(string.Empty, false, embed);
                }
            }
            catch (UnauthorizedException ex)
            {
                var guild = await Bot.Client.GetGuildAsync(ulong.Parse(channelData.GuildId));
                var guildOwner = guild.Owner;

                _logger.Warn($"No permissions to send message to channel [{channelData.ChannelId}], " +
                             $"removing all subscriptions and sending message to {guildOwner.Username} [{guildOwner.Id}]");
                _logger.Warn($"JSON: {ex.JsonMessage}");

                await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));

                var ownerDm = await guildOwner.CreateDmChannelAsync();
                var errorEmbed = _flickrEmbedGenerator.BuildUnauthorizedError();
                await ownerDm.SendMessageAsync(embed: errorEmbed);
            }
            catch (NotFoundException ex)
            {
                _logger.Warn($"Channel [{channelData.ChannelId}] not found, removing all subscriptions");
                _logger.Warn($"JSON: {ex.JsonMessage}");

                await _subscriptionsService.RemoveAllSubscriptionsFromChannelAsync(ulong.Parse(channelData.ChannelId));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Can't send Flickr photo to the channel with id [{channelData.ChannelId}]");
            }
        }
    }
}
