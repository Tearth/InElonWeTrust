using System.Globalization;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class FlickrEmbedGenerator
    {
        public DiscordEmbed Build(CachedFlickrPhoto photo)
        {
            var date = photo.UploadDate.ToString("F", CultureInfo.InvariantCulture);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{photo.Title}",
                Url = $"https://www.flickr.com/photos/spacex/{photo.Id}",
                Color = new DiscordColor(Constants.EmbedColor),
                ImageUrl = photo.Source,
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

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has not enough permissions to post Flickr photos. Check it and subscribe Flickr again.");

            return embed;
        }
    }
}
