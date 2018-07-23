using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class QuoteEmbedGenerator
    {
        public DiscordEmbedBuilder Build(string quote)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField("Elon Musk said:", $"*{quote}*\r\n");

            return embed;
        }
    }
}
