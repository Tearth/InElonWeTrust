using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class UptimeEmbedGenerator
    {
        public DiscordEmbed Build(string formattedTime)
        {
            return new DiscordEmbedBuilder
            {
                Title = "Uptime",
                Description = formattedTime,
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }
    }
}
