using DSharpPlus.Entities;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class FlickrEmbedGenerator
    {
        public DiscordEmbedBuilder Build(CachedFlickrPhoto photo)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ImageUrl = photo.Source
            };

            embed.AddField($"Flickr: {photo.Title} ({photo.UploadDate})", $"https://www.flickr.com/photos/spacex/{photo.Id}");
            return embed;
        }

        public DiscordEmbedBuilder BuildUnauthorizedError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            embed.AddField(":octagonal_sign: Oops!", "It seems that bot have no enough permissions to post Flickr photos. Check it and subscribe Flickr again.");

            return embed;
        }
    }
}
