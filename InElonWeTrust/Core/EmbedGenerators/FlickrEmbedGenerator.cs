using System.Globalization;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class FlickrEmbedGenerator
    {
        public DiscordEmbedBuilder Build(CachedFlickrPhoto photo)
        {
            var date = photo.UploadDate.ToString("F", CultureInfo.InvariantCulture);
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                Title = $"Flickr: {photo.Title} ({date})",
                Url = $"https://www.flickr.com/photos/spacex/{photo.Id}",
                ImageUrl = photo.Source
            };

            return embed;
        }

        public DiscordEmbedBuilder BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot has no enough permissions to post Flickr photos. Check it and subscribe Flickr again.");

            return embed;
        }
    }
}
