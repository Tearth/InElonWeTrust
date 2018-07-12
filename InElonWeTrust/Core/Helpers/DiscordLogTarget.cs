using System;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Settings;
using NLog;
using NLog.Targets;

namespace InElonWeTrust.Core.Helpers
{
    public sealed class DiscordLogTarget : TargetWithLayout
    {
        protected override async void Write(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);

            if (Bot.Client != null)
            {
                var embed = new DiscordEmbedBuilder {Color = new DiscordColor(Constants.EmbedErrorColor)};
                embed.AddField("Error", logMessage.Substring(0, Math.Min(1024, logMessage.Length)));

                var dm = await Bot.Client.CreateDmAsync(await Bot.Client.GetUserAsync(SettingsLoader.Data.OwnerId));
                await dm.SendMessageAsync("", false, embed);
            }
        }
    }
}
