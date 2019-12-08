using System;
using System.Globalization;
using System.Text;
using System.Web;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Extensions;
using InElonWeTrust.Core.Services.Reddit;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class RedditEmbedGenerator
    {
        public DiscordEmbed Build(RedditChildData topic)
        {
            var date = new DateTime().UnixTimeStampToDateTime(topic.Created).ToString("F", CultureInfo.InvariantCulture) + " UTC";

            var embed = new DiscordEmbedBuilder
            {
                Title = HttpUtility.HtmlDecode(topic.Title).ShortenString(230),
                Url = "https://www.reddit.com" + topic.Permalink,
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = topic.Thumbnail == "self" || topic.Thumbnail == "default" ? Constants.SpaceXLogoImage : topic.Thumbnail,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = date
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

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has not enough permissions to post Reddit topic. Check it and subscribe Reddit again.");

            return embed;
        }
    }
}
