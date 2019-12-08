using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class PingEmbedGenerator
    {
        public DiscordEmbed Build(int responseTime)
        {
            return new DiscordEmbedBuilder
            {
                Title = "Pong",
                Description = $"{responseTime} ms",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }
    }
}
