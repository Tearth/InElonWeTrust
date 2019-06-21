using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class ChangelogEmbedGenerator
    {
        public DiscordEmbed Build(string changelog)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                Title = "Changelog",
                Description = changelog
            };

            return embed;
        }
    }
}
