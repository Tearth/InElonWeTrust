using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using InElonWeTrust.Core.Services.BotLists.CommonBotLists;
using InElonWeTrust.Core.Settings;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class CommonBotListsService
    {
        private readonly Timer _statusRefreshTimer;
        private readonly List<BotListUpdater> _botListDefinitions;
        private const int StatusUpdateIntervalMinutes = 1;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CommonBotListsService()
        {
            _botListDefinitions = new List<BotListUpdater>
            {
                new BotListUpdater("https://botlist.space/api/bots/{0}", SettingsLoader.Data.BotlistToken),
                new BotListUpdater("https://botsfordiscord.com/api/v1/bots/{0}", SettingsLoader.Data.BotsForDiscordToken),
                new BotListUpdater("https://bots.discord.pw/api/bots/{0}/stats", SettingsLoader.Data.DiscordPwToken),
            };
            
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
                _logger.Error(ex, "Unable to update bot status on Botlist.space");
            }
        }

        private async Task UpdateStatus()
        {
            foreach (var botList in _botListDefinitions)
            {
                await botList.UpdateStatus(Bot.Client.Guilds.Count);
            }
        }
    }
}
