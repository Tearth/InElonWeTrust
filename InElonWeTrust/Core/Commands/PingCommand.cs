using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":question:", "Misc", "Other strange commands")]
    public class PingCommand
    {
        [Command("Ping")]
        [Description("Ping command")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Ping: {ctx.Client.Ping} ms");
        }
    }
}

