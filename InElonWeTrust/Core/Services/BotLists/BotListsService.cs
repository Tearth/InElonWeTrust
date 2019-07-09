using System;
using System.Collections.Generic;
using System.Timers;
using InElonWeTrust.Core.Settings;
using NLog;

namespace InElonWeTrust.Core.Services.BotLists
{
    public class BotListsService
    {
        private readonly Timer _statusRefreshTimer;
        private readonly List<BotListUpdater> _botListDefinitions;
        private const int StatusUpdateIntervalMinutes = 5;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BotListsService()
        {
            _botListDefinitions = new List<BotListUpdater>
            {
                new BotListUpdater("https://discordbots.org/api/bots/{0}/stats", "server_count", SettingsLoader.Data.DiscordBotsToken),
                new BotListUpdater("https://botsfordiscord.com/api/bot/{0}", "server_count", SettingsLoader.Data.BotsForDiscordToken),
                new BotListUpdater("https://api.botlist.space/v1/bots/{0}", "server_count", SettingsLoader.Data.BotlistToken),
                new BotListUpdater("https://discordsbestbots.xyz/api/bots/{0}/stats", "guilds", SettingsLoader.Data.DiscordsBestBotsToken),
                new BotListUpdater("https://discordbot.world/api/bot/{0}/stats", "guild_count", SettingsLoader.Data.DiscordBotWorldToken),
                new BotListUpdater("https://api.discordbots.group/v1/bot/{0}", "server_count", SettingsLoader.Data.DiscordBotsGroupToken),
                new BotListUpdater("https://discordbotlist.com/api/bots/{0}/stats", "guilds", "Bot " + SettingsLoader.Data.DiscordBotListToken),
                new BotListUpdater("https://bots.ondiscord.xyz/bot-api/bots/{0}/guilds", "guildCount", SettingsLoader.Data.BotsOnDiscordToken),
                new BotListUpdater("https://divinediscordbots.com/bot/{0}/stats", "server_count", SettingsLoader.Data.DivineDiscordBotsToken),
                new BotListUpdater("https://discord.bots.gg/api/v1/bots/{0}/stats", "guildCount", SettingsLoader.Data.DiscordBotsGgToken)
            };

#if !DEBUG
            _statusRefreshTimer = new Timer(StatusUpdateIntervalMinutes * 1000 * 60);
            _statusRefreshTimer.Elapsed += StatusRefreshTimer_ElapsedAsync;
            _statusRefreshTimer.Start();
#endif
        }

        private async void StatusRefreshTimer_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            foreach (var botList in _botListDefinitions)
            {
                try
                {
                    await botList.UpdateStatusAsync(Bot.Client.Guilds.Count);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warn, $"Can't update {botList.Link} list: {ex.Message}");
                }
            }

            _logger.Log(LogLevel.Info, "Botlist status updated");
        }
    }
}
