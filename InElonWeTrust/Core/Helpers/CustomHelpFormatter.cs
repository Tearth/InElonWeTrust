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
using InElonWeTrust.Core.Settings;
using Microsoft.Extensions.Primitives;

namespace InElonWeTrust.Core.Helpers
{
    public class CustomHelpFormatter : IHelpFormatter
    {
        private string _commandName;
        private string _commandDescription;
        private List<string> _aliases;
        private List<string> _parameters;
        private Dictionary<CommandsAttribute, List<string>> _subCommands;

        public CustomHelpFormatter()
        {
            _aliases = new List<string>();
            _parameters = new List<string>();
            _subCommands = new Dictionary<CommandsAttribute, List<string>>();
        }

        public IHelpFormatter WithCommandName(string name)
        {
            _commandName = name;
            return this;
        }

        public IHelpFormatter WithDescription(string description)
        {
            _commandDescription = description;
            return this;
        }

        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            foreach (var argument in arguments)
            {
                var argumentBuilder = new StringBuilder();
                argumentBuilder.Append($"`{argument.Name}`: {argument.Description}");

                if (argument.DefaultValue != null)
                {
                    argumentBuilder.Append($" Default value: {argument.DefaultValue}");
                }

                _parameters.Add(argumentBuilder.ToString());
            }

            return this;
        }

        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            foreach (var alias in aliases)
            {
                _aliases.Add($"`{alias}`");
            }

            return this;
        }

        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyTypes = assembly.GetTypes();

            foreach (var type in assemblyTypes)
            {
                var attributes = type.GetCustomAttributes();
                var commandGroupAttribute = attributes.FirstOrDefault(p => p is CommandsAttribute);

                if (commandGroupAttribute != null)
                {
                    var groupAttribute = (CommandsAttribute)attributes.First(p => p is CommandsAttribute);
                    var commandHandlers = type.GetMethods();

                    foreach (var method in commandHandlers)
                    {
                        var methodAttributes = method.GetCustomAttributes();
                        var commandAttribute = (CommandAttribute)methodAttributes.FirstOrDefault(p => p is CommandAttribute);

                        if (commandAttribute != null && !methodAttributes.Any(p => p is HiddenCommandAttribute))
                        {
                            if (!_subCommands.ContainsKey(groupAttribute))
                            {
                                _subCommands.Add(groupAttribute, new List<string>());
                            }

                            _subCommands[groupAttribute].Add($"`{commandAttribute.Name}`");
                        }
                    }

                    if (_subCommands.ContainsKey(groupAttribute))
                    {
                        _subCommands[groupAttribute] = _subCommands[groupAttribute].OrderBy(p => p).ToList();
                    }
                }
            }

            return this;
        }

        public IHelpFormatter WithGroupExecutable()
        {
            return this;
        }

        public CommandHelpMessage Build()
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
                               "more detailed information about the specified command. The bot is case-insensitive and have " +
                               "no troubles with spaces between the prefix and the command.\r\n\r\n");
            helpBuilder.Append(":newspaper: Join to **[InElonWeTrust bot support](https://discord.gg/cf6ZPZ3)**!\r\n");
            helpBuilder.Append(":wrench: **[GitHub](https://github.com/Tearth/InElonWeTrust)** - yes, we love open source!\r\n");
            helpBuilder.Append(":love_letter: **[Invite me!](https://discordapp.com/api/oauth2/authorize?client_id=462742130016780337&permissions=26688&scope=bot) to your server!**");

            embed.AddField(":rocket: In Elon We Trust, In Thrust We Trust", helpBuilder.ToString());
            helpBuilder.Clear();

            var orderedSubCommands = _subCommands.OrderBy(p => p.Key.Group).ToList();
            foreach (var group in orderedSubCommands)
            {
                helpBuilder.Append($"\r\n\r\n");
                helpBuilder.Append($"{group.Key.Icon} **{group.Key.Group}** *({group.Key.Description}):*\r\n");
                helpBuilder.Append($"{string.Join(", ", group.Value)}");
            }

            embed.AddField("------------------------------", helpBuilder.ToString());
            embed.AddField("------------------------------", "*Happy launches!*");
            return new CommandHelpMessage(string.Empty, embed);
        }

        private CommandHelpMessage BuildCommandHelp(DiscordEmbedBuilder embed)
        {
            embed.AddField(_commandName, _commandDescription);

            if (_aliases.Count > 0) embed.AddField("Aliases", string.Join(", ", _aliases));
            if (_parameters.Count > 0) embed.AddField("Parameters", string.Join("\r\n", _parameters));

            return new CommandHelpMessage(string.Empty, embed);
        }
    }
}
