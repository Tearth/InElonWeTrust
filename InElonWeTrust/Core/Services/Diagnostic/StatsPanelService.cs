using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Helpers.Extensions;
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
            using (var httpClient = new HttpClient())
            {
                var botStats = new BotStats
                {
                    BotId = SettingsLoader.Data.BotId.ToString(),
                    GuildsCount = Bot.Client.Guilds.Count,
                    MembersCount = Bot.Client.Guilds.Sum(p => p.Value.MemberCount),
                    ExecutedCommandsCount = await GetExecutedCommandsCountAsync()
                };

                var json = JsonConvert.SerializeObject(botStats);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                requestContent.Headers.ContentType.CharSet = string.Empty;

                var response = await httpClient.PostWithRetriesAsync("https://discord.tearth.dev:4000/api/stats", requestContent);
                if (response.StatusCode != HttpStatusCode.OK)
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
