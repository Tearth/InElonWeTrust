using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using InElonWeTrust.Core.Commands;
using InElonWeTrust.Core.Configs;

namespace InElonWeTrust.Core
{
    public class Bot
    {
        public DiscordClient Client { get; set; }
        public CommandsNextModule Commands { get; set; }

        public async Task Run()
        {
            Client = new DiscordClient(GetClientConfiguration());
            Client.SetWebSocketClient<WebSocket4NetCoreClient>();

            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;

            Commands = Client.UseCommandsNext(GetCommandsConfiguration());
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            Commands.RegisterCommands<PingCommand>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private DiscordConfiguration GetClientConfiguration()
        {
            return new DiscordConfiguration
            {
                Token = ConfigManager.Data.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
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
            foreach (var prefix in ConfigManager.Data.Prefixes)
            {
                if (msg.Content.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Task.FromResult(prefix.Length);
                }
            }

            return Task.FromResult(-1);
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", "Client is ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            return Task.CompletedTask;
        }
    }
}
