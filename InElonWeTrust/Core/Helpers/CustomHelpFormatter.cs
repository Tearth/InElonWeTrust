using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;

namespace InElonWeTrust.Core.Helpers
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        private string _commandName;
        private string _commandDescription;
        private readonly List<string> _aliases;
        private readonly List<string> _parameters;
        private readonly Dictionary<GroupType, List<string>> _subCommands;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _aliases = new List<string>();
            _parameters = new List<string>();
            _subCommands = new Dictionary<GroupType, List<string>>();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _commandName = command.Name;
            _commandDescription = command.Description;

            if (command.Overloads.Count > 0)
            {
                foreach (var argument in command.Overloads[0].Arguments)
                {
                    var argumentBuilder = new StringBuilder();
                    argumentBuilder.Append($"`{argument.Name}`: {argument.Description}");

                    if (argument.DefaultValue != null)
                    {
                        argumentBuilder.Append($" Default value: {argument.DefaultValue}");
                    }

                    _parameters.Add(argumentBuilder.ToString());
                }
            }

            foreach (var alias in command.Aliases)
            {
                _aliases.Add($"`{alias}`");
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyTypes = assembly.GetTypes();

            foreach (var type in assemblyTypes)
            {
                var attributes = type.GetCustomAttributes().ToList();
                var commandGroupAttribute = attributes.FirstOrDefault(p => p is CommandsAttribute);

                if (commandGroupAttribute != null)
                {
                    var groupAttribute = (CommandsAttribute)attributes.First(p => p is CommandsAttribute);
                    var commandHandlers = type.GetMethods().ToList();

                    foreach (var method in commandHandlers)
                    {
                        var methodAttributes = method.GetCustomAttributes().ToList();
                        var commandAttribute = (CommandAttribute)methodAttributes.FirstOrDefault(p => p is CommandAttribute);

                        if (commandAttribute != null && !methodAttributes.Any(p => p is HiddenCommandAttribute))
                        {
                            if (!_subCommands.ContainsKey(groupAttribute.GroupType))
                            {
                                _subCommands.Add(groupAttribute.GroupType, new List<string>());
                            }

                            _subCommands[groupAttribute.GroupType].Add($"`{commandAttribute.Name}`");
                        }

                        if (_subCommands.ContainsKey(groupAttribute.GroupType))
                        {
                            _subCommands[groupAttribute.GroupType] = _subCommands[groupAttribute.GroupType].OrderBy(p => p).ToList();
                        }
                    }
                }
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor)
            };

            return _commandName == null ? BuildGeneralHelp(embed) : BuildCommandHelp(embed);
        }

        private CommandHelpMessage BuildGeneralHelp(DiscordEmbedBuilder embed)
        {
            var helpBuilder = new StringBuilder();
            helpBuilder.Append("Example usage: `e!ping`, `e! ping`, `elon! ping`. Type `e!help <command_name>` to get " +
                               "more detailed information about the specified command. The bot is case-insensitive and has " +
                               "no troubles with spaces between the prefix and the command.\r\n\r\n");
            helpBuilder.Append(":newspaper: Join to **[InElonWeTrust bot support](https://discord.gg/cf6ZPZ3)**\r\n");
            helpBuilder.Append(":wrench: **[GitHub](https://github.com/Tearth/InElonWeTrust)** - absolutely uninteresting stuff\r\n\u200b\r\n");
            helpBuilder.Append(":computer: Profile on **[discordbots.org](https://discordbots.org/bot/462742130016780337)** and **[bots.discord.pw](https://bots.discord.pw/bots/462742130016780337)**\r\n");
            helpBuilder.Append(":love_letter: **[Invite me](https://discordapp.com/oauth2/authorize?client_id=462742130016780337&permissions=27712&scope=bot) to your server**");

            embed.AddField(":rocket: In Elon We Trust, In Thrust We Trust", helpBuilder.ToString());
            helpBuilder.Clear();

            var orderedSubCommands = _subCommands.OrderBy(p => p.Key).ToList();
            foreach (var group in orderedSubCommands)
            {
                var groupDescription = GetGroupDescription(group.Key);

                helpBuilder.Append( "\r\n\r\n");
                helpBuilder.Append($"{groupDescription.Icon} **{groupDescription.Group}** *({groupDescription.Description}):*\r\n");
                helpBuilder.Append($"{string.Join(", ", group.Value)}");

                embed.AddField("\u200b", helpBuilder.ToString());
                helpBuilder.Clear();
            }

            embed.AddField("\u200b", "*Happy landings!*");
            return new CommandHelpMessage(string.Empty, embed);
        }

        private CommandHelpMessage BuildCommandHelp(DiscordEmbedBuilder embed)
        {
            embed.AddField(_commandName, _commandDescription);

            if (_aliases.Count > 0)
            {
                embed.AddField("Aliases", string.Join(", ", _aliases));
            }

            if (_parameters.Count > 0)
            {
                embed.AddField("Parameters", string.Join("\r\n", _parameters));
            }

            return new CommandHelpMessage(string.Empty, embed);
        }

        private GroupTypeDescriptionAttribute GetGroupDescription(GroupType group)
        {
            var groupType = group.GetType();
            var enumMember = groupType.GetMember(group.ToString())[0];
            var attributes = enumMember.GetCustomAttributes(typeof(GroupTypeDescriptionAttribute));
            var groupTypeDescription = (GroupTypeDescriptionAttribute)attributes.ElementAt(0);

            return groupTypeDescription;
        }
    }
}
