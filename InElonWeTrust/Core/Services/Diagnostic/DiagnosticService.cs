using System.Linq;
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
        private readonly Timer _displayDiagnosticTimer;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int IntervalMinutes = 30;

        public DiagnosticService()
        {
            _displayDiagnosticTimer = new Timer(IntervalMinutes * 60 * 1000);
            _displayDiagnosticTimer.Elapsed += DisplayDiagnosticTimerOnElapsed;
            _displayDiagnosticTimer.Start();
        }

        public void AddExecutedCommand(Command command, DiscordGuild guild)
        {
            using (var databaseContext = new DatabaseContext())
            {
                var commandData = databaseContext.CommandsStats.FirstOrDefault(p => p.CommandName == command.Name);
                if (commandData == null)
                {
                    databaseContext.CommandsStats.Add(new CommandStats(command.Name, 1));
                }
                else
                {
                    commandData.ExecutionsCount++;
                }

                var fixedGuildId = guild.Id.ToString();
                var guildData = databaseContext.GuildsStats.FirstOrDefault(p => p.GuildId == fixedGuildId);

                if (guildData == null)
                {
                    databaseContext.GuildsStats.Add(new GuildStats(fixedGuildId, 1));
                }
                else
                {
                    guildData.CommandExecutionsCount++;
                }

                databaseContext.SaveChanges();
            }
        }

        private void DisplayDiagnosticTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var guildsCount = Bot.Client.Guilds.Count;
            var membersCount = Bot.Client.Guilds.Values.Sum(p => p.MemberCount);

            _logger.Info($"Diagnostic info: {guildsCount} guilds, {membersCount} members");

            using (var databaseContext = new DatabaseContext())
            {
                var guildsStats = databaseContext.GuildsStats.OrderByDescending(p => p.CommandExecutionsCount).ToList();
                var commandsStats = databaseContext.CommandsStats.OrderByDescending(p => p.ExecutionsCount).ToList();

                _logger.Info("Commands per guild usage:");
                foreach (var guild in guildsStats)
                {
                    var guildName = Bot.Client.Guilds.First(p => p.Key == ulong.Parse(guild.GuildId)).Value.Name;
                    _logger.Info($"    {guildName}: {guild.CommandExecutionsCount}");
                }
                _logger.Info($" Total: {guildsStats.Sum(p => p.CommandExecutionsCount)}");

                _logger.Info("Commands executions:");
                foreach (var command in commandsStats)
                {
                    _logger.Info($"    {command.CommandName}: {command.ExecutionsCount}");
                }
                _logger.Info($" Total: {commandsStats.Sum(p => p.ExecutionsCount)}");
            }
        }
    }
}
