using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class QuoteEmbedGenerator
    {
        public DiscordEmbed Build(string quote)
        {
            return new DiscordEmbedBuilder
            {
                Title = "Elon Musk said",
                Description = $"*{quote}*",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }
    }
}
