using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class PingCommand
    {
        private DateTime _startTime;

        public PingCommand()
        {
            _startTime = DateTime.Now;
        }

        [Command("Ping")]
        [Description("Ping command")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Ping: {ctx.Client.Ping} ms");
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

