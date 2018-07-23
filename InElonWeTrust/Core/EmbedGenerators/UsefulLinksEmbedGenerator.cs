using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database.Models;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class UsefulLinksEmbedGenerator
    {
        public DiscordEmbedBuilder Build(List<UsefulLink> links)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Useful links",
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var firstColumnContentBuilder = new StringBuilder();
            var secondColumnContentBuilder = new StringBuilder();

            var firstColumn = links.GetRange(0, links.Count / 2);
            var secondColumn = links.GetRange(links.Count / 2, links.Count - links.Count / 2);

            foreach (var link in firstColumn)
            {
                firstColumnContentBuilder.Append($"[{link.Name}]({link.Link})\r\n");
            }

            foreach (var link in secondColumn)
            {
                secondColumnContentBuilder.Append($"[{link.Name}]({link.Link})\r\n");
            }

            embedBuilder.AddField("\u200b", firstColumnContentBuilder.ToString(), true);
            embedBuilder.AddField("\u200b", secondColumnContentBuilder.ToString(), true);

            return embedBuilder;
        }
    }
}
