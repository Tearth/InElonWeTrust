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
    public class ResetTimeZoneCommand : BaseCommandModule
    {
        private readonly TimeZoneService _timeZoneService;

        public ResetTimeZoneCommand(TimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        [Command("ResetTimeZone")]
        [Description("Reset local timezone (local time won't be shown again).")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task ResetTimeZoneAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await _timeZoneService.ResetTimeZoneAsync(ctx.Guild.Id);
            await ctx.RespondAsync("Time zone has been reset.");
        }
    }
}
