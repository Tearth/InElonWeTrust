﻿using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class AvatarEmbedGenerator
    {
        public DiscordEmbed Build(string user, string avatar)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.ImageUrl = avatar;
            return embed;
        }
    }
}
