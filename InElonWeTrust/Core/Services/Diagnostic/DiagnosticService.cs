using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using NLog;

namespace InElonWeTrust.Core.Services.Diagnostic
{
    public class DiagnosticService
    {
        private readonly StatsPanelService _statsPanelService;
        private readonly Timer _displayDiagnosticTimer;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int DisplayDiagnosticIntervalMinutes = 30;

        public DiagnosticService()
        {
            _statsPanelService = new StatsPanelService();

            _displayDiagnosticTimer = new Timer(DisplayDiagnosticIntervalMinutes * 60 * 1000);

#if !DEBUG
            _displayDiagnosticTimer.Elapsed += DisplayDiagnosticTimerOnElapsed;
            _displayDiagnosticTimer.Start();
#endif
        }

        public async Task AddExecutedCommand(Command command, DiscordGuild guild)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var commandData = databaseContext.CommandsStats.FirstOrDefault(p => p.CommandName == command.Name.ToLower());
                if (commandData == null)
                {
                    databaseContext.CommandsStats.Add(new CommandStats(command.Name.ToLower(), 1));
                }
                else
                {
                    commandData.ExecutionsCount++;
                }

                var fixedGuildId = guild.Id.ToString();
                var guildData = databaseContext.GuildsStats.FirstOrDefault(p => p.GuildId == fixedGuildId);

                if (guildData == null)
                {
                    await databaseContext.GuildsStats.AddAsync(new GuildStats(fixedGuildId, 1));
                }
                else
                {
                    guildData.CommandExecutionsCount++;
                }

                await databaseContext.SaveChangesAsync();
            }
        }

        private void DisplayDiagnosticTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                DisplayDiagnostic();
                _statsPanelService.PostStats();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Can't display diagnostic data");
            }
        }

        private void DisplayDiagnostic()
        {
            var guildsCount = Bot.Client.Guilds.Count;
            var membersCount = Bot.Client.Guilds.Values.Sum(p => p.MemberCount);

            _logger.Info($"Diagnostic info: {guildsCount} guilds, {membersCount} members");

            using (var databaseContext = new DatabaseContext())
            {
                var guildsStats = databaseContext.GuildsStats.OrderByDescending(p => p.CommandExecutionsCount).ToList();
                _logger.Info($" ==== Total commands usage: {guildsStats.Sum(p => p.CommandExecutionsCount)} ==== ");
            }
        }
    }
}
