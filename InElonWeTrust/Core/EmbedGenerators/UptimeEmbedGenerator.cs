using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class UptimeEmbedGenerator
    {
        public DiscordEmbed Build(string formattedTime)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField("Uptime", formattedTime);

            return embed;
        }
    }
}
