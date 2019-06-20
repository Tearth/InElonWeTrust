using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Services.TimeZone;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.TimeZone)]
    public class SetTimeZoneCommand : BaseCommandModule
    {
        private readonly TimeZoneService _timeZoneService;

        public SetTimeZoneCommand(TimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        [Command("SetTimeZone")]
        [Aliases("TimeZone")]
        [Description("Set local timezone. Complete list is available on https://en.wikipedia.org/wiki/List_of_tz_database_time_zones#List (column TZ database name, e.g `e!SetTimeZone Europe/Warsaw`).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task SetTimeZone(CommandContext ctx, [Description("Name of the timezone")] string timeZoneName)
        {
            await ctx.TriggerTypingAsync();

            if (_timeZoneService.TimeZoneExists(ctx.RawArgumentString))
            {
                await _timeZoneService.SetTimeZoneAsync(ctx.Guild.Id, timeZoneName);
                await ctx.RespondAsync($"Time zone has been set to {ctx.RawArgumentString}.");
            }
            else
            {
                await ctx.RespondAsync("Invalid name of time zone.");
            }
        }
    }
}
