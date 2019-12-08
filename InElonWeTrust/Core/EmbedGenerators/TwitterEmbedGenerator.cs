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
        public DiscordEmbed Build(CachedTweet tweet)
        {
            var polandTimeZone = TZConvert.GetTimeZoneInfo("Europe/Warsaw");
            var createdAtWithKind = DateTime.SpecifyKind(tweet.CreatedAt, DateTimeKind.Unspecified);
            var createdAtUtc = TimeZoneInfo.ConvertTimeToUtc(createdAtWithKind, polandTimeZone);
            var date = createdAtUtc.ToString("F", CultureInfo.InvariantCulture);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{tweet.CreatedByDisplayName} Twitter",
                Url = tweet.Url,
                Description = HttpUtility.HtmlDecode(tweet.FullText),
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = tweet.AvatarUrl,
                ImageUrl = tweet.ImageUrl,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{date} UTC"
                }
            };

            return embed;
        }

        public DiscordEmbed BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has not enough permissions to post tweet. Check it and subscribe Twitter again.");

            return embed;
        }
    }
}
