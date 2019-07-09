using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Miscellaneous)]
    public class PreburnerCommand : BaseCommandModule
    {
        [RequireOwner]
        [Hidden, Command("Preburner"), Aliases("Preburn")]
        [Description("Preburn all non hidden commands to speed up their execution.")]
        public async Task PreburnerAsync(CommandContext ctx)
        {
            var commands = Bot.Client.GetCommandsNext().RegisteredCommands;
            var helpCommand = commands.First(p => p.Key == "help").Value;
            var commandsWithoutDuplicates = commands
                .GroupBy(p => p.Value)
                .Where(p => !p.Key.IsHidden)
                .Select(p => p.Key)
                .ToList();

            Bot.LogExecutedCommands = false;

            foreach (var command in commandsWithoutDuplicates)
            {
                var parameters = GetParametersForCommand(command);
                var helpMessage = $"{ctx.Prefix}help {command.Name}";
                var commandMessage = $"{ctx.Prefix}{command.Name} {parameters}";

                var helpFakeContext = ctx.CommandsNext.CreateFakeContext(ctx.User, ctx.Channel, helpMessage, ctx.Prefix, helpCommand, command.Name);
                await ctx.CommandsNext.ExecuteCommandAsync(helpFakeContext);

                var commandFakeContext = ctx.CommandsNext.CreateFakeContext(ctx.User, ctx.Channel, commandMessage, ctx.Prefix, command, parameters);
                await ctx.CommandsNext.ExecuteCommandAsync(commandFakeContext);
            }

            Bot.LogExecutedCommands = true;
        }

        private string GetParametersForCommand(Command command)
        {
            var output = string.Empty;
            foreach (var parameter in command.Overloads[0].Arguments)
            {
                switch (parameter.Name)
                {
                    case "id": output += "10 "; break;
                    case "timeZoneName": output += "Europe/Warsaw "; break;
                    case "coreSerial": output += "B1050 "; break;
                    case "orbitType": output += "GEO "; break;
                }
            }

            return output;
        }
    }
}