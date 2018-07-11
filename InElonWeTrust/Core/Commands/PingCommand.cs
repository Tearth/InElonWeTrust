using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Misc")]
    public class PingCommand
    {
        [Command("ping")]
        [Description("Ping command")]
        public async Task Ping(CommandContext ctx)
        {
            int a = 0;
            int b = 10 / a;

            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync($"Ping: {ctx.Client.Ping} ms");
        }
    }
}

