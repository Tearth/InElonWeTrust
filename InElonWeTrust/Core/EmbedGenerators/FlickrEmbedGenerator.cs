using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    }
}
