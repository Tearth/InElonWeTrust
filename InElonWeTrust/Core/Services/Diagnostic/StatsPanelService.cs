using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;

namespace InElonWeTrust.Core.Services.Diagnostic
{
    public class StatsPanelService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task PostStatsAsync()
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var botStats = new BotStats
                {
                    BotId = SettingsLoader.Data.BotId.ToString(),
                    GuildsCount = Bot.Client.Guilds.Count,
                    MembersCount = Bot.Client.Guilds.Sum(p => p.Value.MemberCount),
                    ExecutedCommandsCount = await GetExecutedCommandsCountAsync()
                };

                var json = JsonConvert.SerializeObject(botStats);
                var response = webClient.UploadString("http://localhost:4000/api/stats", json);

                if (response != string.Empty)
                {
                    _logger.Error("Can't send diagnostic data to panel");
                }
            }
        }

        private async Task<int> GetExecutedCommandsCountAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                return await databaseContext.GuildsStats
                    .OrderByDescending(p => p.CommandExecutionsCount)
                    .SumAsync(p => p.CommandExecutionsCount);
            }
        }
    }
}
