using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class PreburnerCommand : BaseCommandModule
    {
        [RequireOwner]
        [Hidden, Command("Preburner")]
        [Description("Preburn all non hidden commands to speed up their execution.")]
        public async Task PreburnerAsync(CommandContext ctx)
        {
            var commands = Bot.Client.GetCommandsNext().RegisteredCommands;
            var commandsWithoutDuplicates = commands
                .GroupBy(p => p.Value)
                .Where(p => !p.Key.IsHidden)
                .Select(p => p.Key)
                .ToList();

            Bot.LogExecutedCommands = false;

            foreach (var command in commandsWithoutDuplicates)
            {
                var fakeContext = ctx.CommandsNext.CreateFakeContext(ctx.User, ctx.Channel, $"{ctx.Prefix}{command.Name} 10", ctx.Prefix, command, "10");
                await ctx.CommandsNext.ExecuteCommandAsync(fakeContext);
            }

            Bot.LogExecutedCommands = true;
        }
    }
}