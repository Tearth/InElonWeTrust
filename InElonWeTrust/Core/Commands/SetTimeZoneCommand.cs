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
        private const string SetTimeZoneParameterDescription = 
            "Name of the timezone. Complete list is available on " +
            "[Wikipedia page](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones#List) " +
            "(column \"**TZ database name**\", e.g. `e!SetTimeZone Europe/Warsaw` or `e!SetTimeZone Etc/GMT+11`)";

        public SetTimeZoneCommand(TimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        [Command("SetTimeZone"), Aliases("TimeZone")]
        [Description("Set a local timezone (local time will be displayed in the launch information).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task SetTimeZoneAsync(CommandContext ctx, [Description(SetTimeZoneParameterDescription), RemainingText] string timeZoneName)
        {
            await ctx.TriggerTypingAsync();

            if (_timeZoneService.TimeZoneExists(timeZoneName))
            {
                await _timeZoneService.SetTimeZoneAsync(ctx.Guild.Id, timeZoneName);
                await ctx.RespondAsync($"Time zone has been set to {ctx.RawArgumentString}. You can reset it in the future by `e!ResetTimeZone`.");
            }
            else
            {
                await ctx.RespondAsync("Invalid name of the time zone. Please type `e!help SetTimeZone` to get more information about time zone format.");
            }
        }
    }
}
