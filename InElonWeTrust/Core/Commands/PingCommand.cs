using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace InElonWeTrust.Core.Commands
{
    public class PingCommand
    {
        [Command("ping")]
        [Description("Ping command")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Ping: {ctx.Client.Ping} ms");
        }
    }
}
