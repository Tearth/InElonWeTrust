using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class QuoteEmbedGenerator
    {
        public DiscordEmbed Build(string quote)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField("Elon Musk said", $"*{quote}*\r\n");

            return embed;
        }
    }
}
