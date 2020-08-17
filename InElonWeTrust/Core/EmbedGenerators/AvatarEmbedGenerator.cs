using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class AvatarEmbedGenerator
    {
        public DiscordEmbed Build(string user, string avatar)
        {
            return new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ImageUrl = avatar,
                Title = $"{user}'s avatar"
            };
        }
    }
}
