using System.Linq;
using System.Net;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Settings;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.Diagnostic
{
    public class StatsPanelService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void PostStats()
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var botStats = new BotStats
                {
                    BotId = SettingsLoader.Data.BotId.ToString(),
                    GuildsCount = Bot.Client.Guilds.Count,
                    MembersCount = Bot.Client.Guilds.Sum(p => p.Value.MemberCount),
                    ExecutedCommandsCount = GetExecutedCommandsCount()
                };

                var json = JsonConvert.SerializeObject(botStats);
                var response = webClient.UploadString("http://localhost:4000/api/stats", json);

                if (response != string.Empty)
                {
                    _logger.Error("Can't send diagnostic data to panel.");
                }
            }
        }

        private int GetExecutedCommandsCount()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return databaseContext.GuildsStats
                    .OrderByDescending(p => p.CommandExecutionsCount)
                    .Sum(p => p.CommandExecutionsCount);
            }
        }
    }
}
