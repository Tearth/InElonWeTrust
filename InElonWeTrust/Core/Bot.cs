using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using InElonWeTrust.Core.Services.Diagnostic;
using InElonWeTrust.Core.Services.Flickr;
using InElonWeTrust.Core.Services.LaunchNotifications;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.Services.Quotes;
using InElonWeTrust.Core.Services.Reddit;
using InElonWeTrust.Core.Services.Subscriptions;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Services.UsefulLinks;
using InElonWeTrust.Core.Services.UserLaunchSubscriptions;
using InElonWeTrust.Core.Settings;
using InElonWeTrust.Core.TableGenerators;
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
        private DiagnosticService _diagnosticService;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task Run()
        {
            _oddity = new OddityCore();
            _cacheService = new CacheService();
            _diagnosticService = new DiagnosticService();

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
                .Add<LaunchInfoEmbedGenerator>()
                .Add<ChangelogEmbedGenerator>()
                .Add<CompanyInfoEmbedGenerator>()
                .Add<CompanyHistoryTableGenerator>()
                .Add<CompanyHistoryEventEmbedGenerator>()
                .Add<DescriptionService>()
                .Add<ChangelogService>()
                .Add<UsefulLinksService>()
                .Add<FlickrService>()
                .Add<LaunchNotificationsService>()
                .Add<PaginationService>()
                .Add<QuotesService>()
                .Add<RedditService>()
                .Add<SubscriptionsService>()
                .Add<TwitterService>()
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
            _diagnosticService.AddExecutedCommand(e.Command, e.Context.Guild);

            return Task.CompletedTask;
        }

        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            var errorEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            switch (e.Exception)
            {
                case CommandNotFoundException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", "Can't recognize this command, type `e!help` to get full list of them.");
                    break;
                }

                case ArgumentException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", $"Invalid parameter, type `e!help {e.Command.Name}` to get more info.");
                    break;
                }

                case ChecksFailedException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", "You have no permissions to do this action. Remember that some commands (related with subscriptions) requires Manage Messages permission.");
                    break;
                }

                default:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Oops", $"Something strange happened when bot was trying to execute `{e.Command.Name}` command. Owner has been reported about this accident.");
                    break;
                }
            }

            _logger.Error(e.Exception, GetCommandInfo(e.Context));
            e.Context.RespondAsync("", false, errorEmbedBuilder);

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

