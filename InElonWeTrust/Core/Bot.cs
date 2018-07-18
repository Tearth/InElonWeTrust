using System;
using System.Collections.Generic;
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
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Changelog;
using InElonWeTrust.Core.Services.Description;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.Services.Quotes;
using InElonWeTrust.Core.Services.Reddit;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Services.UserLaunchSubscriptions;
using InElonWeTrust.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Oddity;

namespace InElonWeTrust.Core
{
    public class Bot
    {
        public static DiscordClient Client { get; set; }

        private CommandsNextModule _commands;
        private OddityCore _oddity;
        private CacheService _cacheService;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task Run()
        {
            _oddity = new OddityCore();
            _cacheService = new CacheService();

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
            return new CommandsNextConfiguration
            {
                EnableDms = false,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                CustomPrefixPredicate = CustomPrefixPredicate,
                Dependencies = BuildDependencies()
            };
        }

        private DependencyCollection BuildDependencies()
        {
            return new DependencyCollectionBuilder()
                .AddInstance(_oddity)
                .AddInstance(_cacheService)
                .Add<DescriptionService>()
                .Add<ChangelogService>()
                .Add<FlickrService>()
                .Add<LaunchNotificationsService>()
                .Add<PaginationService>()
                .Add<QuotesService>()
                .Add<RedditService>()
                .Add<SubscriptionsService>()
                .Add<TwitterService>()
                .Add<LaunchInfoEmbedGenerator>()
                .Add<UserLaunchSubscriptionsService>()
                .Build();
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
            var infoList = new List<string>();
            infoList.Add($"Guild: {ctx.Guild.Name}");
            infoList.Add($"Channel: {ctx.Channel.Name}");
            infoList.Add($"User: {ctx.User.Username}");
            infoList.Add($"Call: {ctx.Message.Content}");

            return string.Join(", ", infoList);
        }
    }
}
