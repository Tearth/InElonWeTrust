using System;
using System.Globalization;
using System.Text;
using System.Web;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;
using TimeZoneConverter;

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
            
            var polandTimeZone = TZConvert.GetTimeZoneInfo("Europe/Warsaw");
            var createdAtWithKind = DateTime.SpecifyKind(tweet.CreatedAt, DateTimeKind.Unspecified);
            var createdAtUtc = TimeZoneInfo.ConvertTimeToUtc(createdAtWithKind, polandTimeZone);

            var date = createdAtUtc.ToString("F", CultureInfo.InvariantCulture);
            embed.AddField($"Twitter: {tweet.CreatedByDisplayName} at {date} UTC", contentBuilder.ToString());

            return embed;
        }

        public DiscordEmbedBuilder BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has no enough permissions to post tweet. Check it and subscribe Reddit again.");

            return embed;
        }
    }
}
