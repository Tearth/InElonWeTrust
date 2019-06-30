using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Helpers;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class PreburnerCommand : BaseCommandModule
    {
        [Command("Preburner")]
        [Description("Pong")]
        public async Task PreburnerAsync(CommandContext ctx)
        {
            var commands = Bot.Client.GetCommandsNext().RegisteredCommands;
            var commandsWithoutDuplicates = commands.GroupBy(p => p.Value).Select(p => p.Key).ToList();

            foreach (var command in commandsWithoutDuplicates)
            {
                var fakeContext = ctx.CommandsNext.CreateFakeContext(ctx.User, ctx.Channel, $"{ctx.Prefix}{command.Name}", ctx.Prefix, command);
                await ctx.CommandsNext.ExecuteCommandAsync(fakeContext);
            }
        }
    }
}