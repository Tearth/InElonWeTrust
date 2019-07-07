using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace InElonWeTrust.Core.Services.Diagnostic
{
    public class DiagnosticService
    {
        private readonly StatsPanelService _statsPanelService;
#if !DEBUG
        private readonly Timer _displayDiagnosticTimer;
#endif
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private int _lastHandledMessagesCount;

        private const int DisplayDiagnosticIntervalMinutes = 60;

        public DiagnosticService()
        {
            _statsPanelService = new StatsPanelService();

#if !DEBUG
            _displayDiagnosticTimer = new Timer(DisplayDiagnosticIntervalMinutes * 60 * 1000);
            _displayDiagnosticTimer.Elapsed += DisplayDiagnosticTimerOnElapsedAsync;
            _displayDiagnosticTimer.Start();
#endif
        }

        public async Task AddExecutedCommandAsync(Command command, DiscordGuild guild)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var commandData = await databaseContext.CommandsStats.FirstOrDefaultAsync(p => p.CommandName == command.Name.ToLower());
                if (commandData == null)
                {
                    databaseContext.CommandsStats.Add(new CommandStats(command.Name.ToLower(), 1));
                }
                else
                {
                    commandData.ExecutionsCount++;
                }

                var fixedGuildId = guild.Id.ToString();
                var guildData = await databaseContext.GuildsStats.FirstOrDefaultAsync(p => p.GuildId == fixedGuildId);

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

        private async void DisplayDiagnosticTimerOnElapsedAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                DisplayDiagnostic();
                await _statsPanelService.PostStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Can't display diagnostic data");
            }
        }

        private void DisplayDiagnostic()
        {
            var guildsCount = Bot.Client.Guilds.Count;
            var (humans, bots) = CalculateHumansBotsCount();
            var totalMembers = humans + bots;

            _logger.Info($"==== Diagnostic info: " +
                         $"{guildsCount} guilds, " +
                         $"{totalMembers} members " +
                         $"({humans} humans and " +
                         $"{bots} bots) ====");

            using (var databaseContext = new DatabaseContext())
            {
                var guildsStats = databaseContext.GuildsStats.OrderByDescending(p => p.CommandExecutionsCount).ToList();
                _logger.Info($"==== Total commands usage: {guildsStats.Sum(p => p.CommandExecutionsCount)} ====");
                _logger.Info($"==== Handled messages: {Bot.HandledMessagesCount} " +
                             $"({Bot.HandledMessagesCount - _lastHandledMessagesCount} in the last {DisplayDiagnosticIntervalMinutes} minutes) ====");

                _lastHandledMessagesCount = Bot.HandledMessagesCount;
            }
        }

        private (int humans, int bots) CalculateHumansBotsCount()
        {
            var guilds = Bot.Client.Guilds.Values.ToList();
            var humansCountInSmallGuilds = guilds.Where(p => !p.IsLarge).Sum(p => p.Members.Count(m => !m.Value.IsBot));
            var botsCountInSmallGuilds = guilds.Where(p => !p.IsLarge).Sum(p => p.Members.Count(m => m.Value.IsBot));

            var membersInLargeGuilds = guilds.Where(p => p.IsLarge)
                .Select(async p => await p.GetAllMembersAsync())
                .SelectMany(p => p.Result)
                .ToList();

            var humansInLargeGuilds = membersInLargeGuilds.Count(p => !p.IsBot);
            var botsInLargeGuilds = membersInLargeGuilds.Count(p => p.IsBot);

            return (humansCountInSmallGuilds + humansInLargeGuilds, botsCountInSmallGuilds + botsInLargeGuilds);
        }
    }
}
