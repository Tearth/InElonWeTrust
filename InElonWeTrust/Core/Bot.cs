using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Description;
using InElonWeTrust.Core.Settings;
using NLog;

namespace InElonWeTrust.Core
{
    public class Bot
    {
        public static DiscordClient Client { get; set; }

        private CommandsNextModule _commands;
        private DescriptionService _description;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task Run()
        {
            Client = new DiscordClient(GetClientConfiguration());
            Client.SetWebSocketClient<WebSocket4NetCoreClient>();

            Client.Ready += Client_Ready;
            Client.ClientErrored += Client_ClientError;

            _commands = Client.UseCommandsNext(GetCommandsConfiguration());
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;
            _commands.SetHelpFormatter<CustomHelpFormatter>();

            RegisterCommands();

            await Client.ConnectAsync();

            _description = new DescriptionService();
        }

        private DiscordConfiguration GetClientConfiguration()
        {
            return new DiscordConfiguration
            {
                Token = SettingsLoader.Data.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                UseInternalLogHandler = false
            };
        }

        private CommandsNextConfiguration GetCommandsConfiguration()
        {
            return  new CommandsNextConfiguration
            {
                EnableDms = false,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                CustomPrefixPredicate = CustomPrefixPredicate
            };
        }

        private Task<int> CustomPrefixPredicate(DiscordMessage msg)
        {
            foreach (var prefix in SettingsLoader.Data.Prefixes)
            {
                if (msg.Content.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Task.FromResult(prefix.Length);
                }
            }

            return Task.FromResult(-1);
        }

        private void RegisterCommands()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyTypes = assembly.GetTypes();

            var registerCommandsMethod = _commands.GetType().GetMethods().FirstOrDefault(p => p.Name == "RegisterCommands" && p.IsGenericMethod);

            foreach (var type in assemblyTypes)
            {
                var attributes = type.GetCustomAttributes();
                if (attributes.Any(p => p.GetType() == typeof(CommandsAttribute)))
                {
                    var genericRegisterCommandMethod = registerCommandsMethod.MakeGenericMethod(type);
                    genericRegisterCommandMethod.Invoke(_commands, null);
                }
            }
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info("Bot is ready to process events.");
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            _logger.Error(e);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            //e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, Constants.AppName, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.Append($"Guild: {e.Context.Guild.Name}");
            errorBuilder.Append($", Channel: {e.Context.Channel.Name}");
            errorBuilder.Append($", User: {e.Context.User.Username}");
            errorBuilder.Append($", Call: {e.Context.Message.Content}");

            _logger.Error(e.Exception, errorBuilder.ToString());

            return Task.CompletedTask;
        }
    }
}
