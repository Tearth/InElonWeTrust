using System;
using System.Threading.Tasks;
using System.Timers;
using DiscordBotsList.Api;
using InElonWeTrust.Core.Settings;
using NLog;

namespace InElonWeTrust.Core.Services.DiscordBotList
{
    public class DiscordBotListService
    {
        private readonly Timer _statusRefreshTimer;
        private readonly AuthDiscordBotListApi _discordBotListApi;

        private const int StatusUpdateIntervalMinutes = 10;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public DiscordBotListService()
        {
            _discordBotListApi = new AuthDiscordBotListApi(SettingsLoader.Data.BotId, SettingsLoader.Data.DiscordBotListToken);

            _statusRefreshTimer = new Timer(StatusUpdateIntervalMinutes * 60 * 1000);
            _statusRefreshTimer.Elapsed += StatusRefreshTimer_Elapsed;
            _statusRefreshTimer.Start();
        }

        private async void StatusRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await UpdateStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to update bot status on DiscordBots.org");
            }
        }

        private async Task UpdateStatus()
        {
            await _discordBotListApi.UpdateStats(Bot.Client.Guilds.Count);
        }
    }
}
