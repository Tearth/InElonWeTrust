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
                var dm = await Bot.Client.CreateDmAsync(await Bot.Client.GetUserAsync(SettingsLoader.Data.OwnerId));
                await dm.SendMessageAsync(logMessage);
            }
        }
    }
}
