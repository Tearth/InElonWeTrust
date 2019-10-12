using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class TimeZoneEmbedGenerator
    {
        public DiscordEmbed BuildMessageOnSuccess(string timeZone)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", $"Time zone has been set to {timeZone}. You can reset it in the future by typing `e!ResetTimeZone`.");
            return embed;
        }

        public DiscordEmbed BuildMessageOnError()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":octagonal_sign: Error", "Invalid name of the time zone. Please type `e!help SetTimeZone` to get more information about time zone format.");
            return embed;
        }

        public DiscordEmbed BuildMessageOnReset()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            embed.AddField(":rocket: Success!", "Time zone has been reset.");
            return embed;
        }
    }
}
