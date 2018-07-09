using System;
using System.Collections.Generic;
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
    [Commands("Flicks")]
    public class FlickrCommands
    {
        private SubscriptionsService _subscriptions;

        public FlickrCommands()
        {
            _subscriptions = new SubscriptionsService();
        }

        [Command("addflickrchannel")]
        [Aliases("addfc", "afc")]
        [Description("Get random Elon's tweet.")]
        public async Task AddFlickrChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.AddSubscription(ctx.Channel.Id, SubscriptionType.Flickr))
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

        [Command("removeflickrchannel")]
        [Aliases("removefc", "rfc")]
        [Description("Get random Elon's tweet.")]
        public async Task RemoveFlickrChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                ThumbnailUrl = Constants.ThumbnailImage
            };

            if (await _subscriptions.RemoveSubscription(ctx.Channel.Id, SubscriptionType.Flickr))
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
    }
}
