using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class TimeZoneEmbedGenerator
    {
        public DiscordEmbed BuildMessageOnSuccess(string timeZone)
        {
            return new DiscordEmbedBuilder
            {
                Title = ":rocket: Success!",
                Description = $"Time zone has been set to {timeZone}. You can reset it in the future by typing `e!ResetTimeZone`.",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }

        public DiscordEmbed BuildMessageOnError()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":octagonal_sign: Error",
                Description = "Invalid name of the time zone. Please type `e!help SetTimeZone` to get more information about time zone format.",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }

        public DiscordEmbed BuildMessageOnReset()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":rocket: Success!",
                Description = "Time zone has been reset.",
                Color = new DiscordColor(Constants.EmbedColor)
            };
        }
    }
}
