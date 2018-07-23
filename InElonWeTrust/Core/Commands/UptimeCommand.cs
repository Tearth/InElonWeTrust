using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class UptimeCommand
    {
        private readonly DateTime _startTime;

        public UptimeCommand()
        {
            _startTime = DateTime.Now;
        }

        [Command("Uptime")]
        [Description("Get uptime")]
        public async Task Uptime(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Uptime: {DateTime.Now - _startTime:g}");
        }
    }
}
