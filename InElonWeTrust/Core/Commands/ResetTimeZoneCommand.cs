using System.Threading.Tasks;
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
        public TimeZoneService _timeZoneService;

        public ResetTimeZoneCommand(TimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        [Command("ResetTimeZone")]
        [Description("Reset local timezone.")]
        public async Task ResetTimeZone(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await _timeZoneService.ResetTimeZoneAsync(ctx.Guild.Id);
            await ctx.RespondAsync($"Time zone has been reset.");
        }
    }
}
