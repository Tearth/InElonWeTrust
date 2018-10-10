using System;
using System.Globalization;
using System.Text;
using System.Web;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Reddit;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class RedditEmbedGenerator
    {
        public DiscordEmbedBuilder Build(RedditChildData topic)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                Title = $"Reddit: {HttpUtility.HtmlDecode(topic.Title).ShortenString(230)}",
                Url = "https://www.reddit.com" + topic.Permalink,
                ThumbnailUrl = topic.Thumbnail == "self" || topic.Thumbnail == "default" ? Constants.SpaceXLogoImage : topic.Thumbnail
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.Append($"{topic.Author} | {topic.Upvotes} upvotes\r\n");
            contentBuilder.Append(new DateTime().UnixTimeStampToDateTime(topic.Created).ToUniversalTime().ToString("F", CultureInfo.InvariantCulture) + " UTC");

            embed.AddField("\u200b", contentBuilder.ToString());

            return embed;
        }

        public DiscordEmbedBuilder BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has no enough permissions to post Reddit topic. Check it and subscribe Reddit again.");

            return embed;
        }
    }
}
