using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class TwitterEmbedGenerator
    {
        public DiscordEmbedBuilder Build(CachedTweet tweet)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = tweet.AvatarUrl,
                ImageUrl = tweet.ImageUrl
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(HttpUtility.HtmlDecode(tweet.FullText));
            contentBuilder.Append("\r\n\r\n");
            contentBuilder.Append(tweet.Url);

            embed.AddField($"Twitter: {tweet.CreatedByDisplayName} at {tweet.CreatedAt.ToUniversalTime()} UTC", contentBuilder.ToString());

            return embed;
        }

        public DiscordEmbedBuilder BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot have no enough permissions to post tweet. Check it and subscribe Reddit again.");

            return embed;
        }
    }
}
