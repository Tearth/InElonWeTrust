using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class PingEmbedGenerator
    {
        public DiscordEmbed Build(int responseTime)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField("Pong", $"{responseTime} ms");

            return embed;
        }
    }
}
