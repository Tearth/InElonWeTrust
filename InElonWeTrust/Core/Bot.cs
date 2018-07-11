﻿using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
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
            Client.Heartbeated += Client_Heartbeated;
            Client.ClientErrored += Client_ClientError;
            Client.SocketErrored += Client_SocketErrored;

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
            _logger.Info("In Elon We Trust, In Thrust We Trust.");
            return Task.CompletedTask;
        }

        private Task Client_Heartbeated(HeartbeatEventArgs e)
        {
            _logger.Info($"Heartbeat - ping: {e.Ping} ms");
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            _logger.Error(e.Exception, $"Event Name: {e.EventName}");
            return Task.CompletedTask;
        }

        private Task Client_SocketErrored(SocketErrorEventArgs e)
        {
            _logger.Error(e.Exception);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            _logger.Info(GetCommandInfo(e.Context));
            return Task.CompletedTask;
        }

        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Command?.Name != "help" && !(e.Exception is CommandNotFoundException))
            {
                _logger.Error(e.Exception, GetCommandInfo(e.Context));
            }

            return Task.CompletedTask;
        }

        private string GetCommandInfo(CommandContext ctx)
        {
            var infoBuilder = new StringBuilder();
            infoBuilder.Append($"Guild: {ctx.Guild.Name}");
            infoBuilder.Append($", Channel: {ctx.Channel.Name}");
            infoBuilder.Append($", User: {ctx.User.Username}");
            infoBuilder.Append($", Call: {ctx.Message.Content}");

            return infoBuilder.ToString();
        }
    }
}
