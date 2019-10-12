using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.EmbedGenerators;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class PingCommand : BaseCommandModule
    {
        private readonly PingEmbedGenerator _pingEmbedGenerator;

        public PingCommand(PingEmbedGenerator pingEmbedGenerator)
        {
            _pingEmbedGenerator = pingEmbedGenerator;
        }

        [Command("Ping")]
        [Description("Pong")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var embed = _pingEmbedGenerator.Build(ctx.Client.Ping);

            await ctx.RespondAsync(embed: embed);
        }
    }
}

