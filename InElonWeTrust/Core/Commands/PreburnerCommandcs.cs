using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using InElonWeTrust.Core.Attributes;
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
            var commandsWithoutDuplicates = commands
                .GroupBy(p => p.Value)
                .Where(p => !p.Key.IsHidden)
                .Select(p => p.Key)
                .ToList();

            Bot.LogExecutedCommands = false;

            foreach (var command in commandsWithoutDuplicates)
            {
                var parameters = GetParametersForCommand(command);
                var message = $"{ctx.Prefix}{command.Name} {parameters}";

                var fakeContext = ctx.CommandsNext.CreateFakeContext(ctx.User, ctx.Channel, message, ctx.Prefix, command, parameters);
                await ctx.CommandsNext.ExecuteCommandAsync(fakeContext);
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