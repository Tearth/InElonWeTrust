using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Services.TimeZone;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.TimeZone)]
    public class TimeZoneCommands : BaseCommandModule
    {
        private readonly TimeZoneService _timeZoneService;
        private readonly TimeZoneEmbedGenerator _timeZoneEmbedGenerator;
        private const string SetTimeZoneParameterDescription = 
            "Name of the timezone. Complete list is available on " +
            "[Wikipedia page](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones#List) " +
            "(column \"**TZ database name**\", e.g. `e!SetTimeZone Europe/Warsaw` or `e!SetTimeZone Etc/GMT+11`)";

        public TimeZoneCommands(TimeZoneService timeZoneService, TimeZoneEmbedGenerator timeZoneEmbedGenerator)
        {
            _timeZoneService = timeZoneService;
            _timeZoneEmbedGenerator = timeZoneEmbedGenerator;
        }

        [Command("SetTimeZone"), Aliases("TimeZone")]
        [Description("Set a local timezone (local time will be displayed in the launch information, e.g. `e!NextLaunch`).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task SetTimeZoneAsync(CommandContext ctx, [Description(SetTimeZoneParameterDescription), RemainingText] string timeZoneName)
        {
            await ctx.TriggerTypingAsync();

            if (_timeZoneService.TimeZoneExists(timeZoneName))
            {
                await _timeZoneService.SetTimeZoneAsync(ctx.Guild.Id, timeZoneName);

                var embed = _timeZoneEmbedGenerator.BuildMessageOnSuccess(ctx.RawArgumentString);
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                var embed = _timeZoneEmbedGenerator.BuildMessageOnError();
                await ctx.RespondAsync(embed: embed);
            }
        }

        [Command("ResetTimeZone")]
        [Description("Reset local timezone (local time won't be shown again).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ResetTimeZoneAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await _timeZoneService.ResetTimeZoneAsync(ctx.Guild.Id);

            var embed = _timeZoneEmbedGenerator.BuildMessageOnReset();
            await ctx.RespondAsync(embed: embed);
        }
    }
}
