﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.WebSocket;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.BotLists;
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
using InElonWeTrust.Core.Services.TimeZone;
using InElonWeTrust.Core.Services.Twitter;
using InElonWeTrust.Core.Services.UsefulLinks;
using InElonWeTrust.Core.Services.UserLaunchSubscriptions;
using InElonWeTrust.Core.Settings;
using InElonWeTrust.Core.TableGenerators;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using NLog;
using Oddity;
using Oddity.API.Builders;

namespace InElonWeTrust.Core
{
    public class Bot
    {
        public static DiscordClient Client { get; set; }
        public static bool LogExecutedCommands { get; set; }

        private readonly CommandsNextExtension _commands;
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;
        private readonly DiagnosticService _diagnosticService;
        //private WatchdogService _watchdog;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Bot()
        {
            _oddity = new OddityCore();
            _cacheService = new CacheService();
            _diagnosticService = new DiagnosticService();
            //_watchdog = new WatchdogService();

            _oddity.OnRequestSend += Oddity_OnRequestSend;
            _oddity.OnResponseReceive += Oddity_OnResponseReceive;
            _oddity.OnDeserializationError += Oddity_OnDeserializationError;

            Client = new DiscordClient(GetClientConfiguration());
            LogExecutedCommands = true;

            Client.Ready += Client_Ready;
            Client.Heartbeated += Client_Heartbeat;
            Client.GuildCreated += Client_GuildCreated;
            Client.GuildDeleted += Client_GuildDeleted;
            Client.ClientErrored += Client_ClientError;
            Client.SocketErrored += Client_SocketError;
            Client.SocketClosed += Client_SocketClosed;

            _commands = Client.UseCommandsNext(GetCommandsConfiguration());
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandError;
            _commands.SetHelpFormatter<CustomHelpFormatter>();

            RegisterCommands();
        }

        public async Task Run()
        {
            //_watchdog.Start();
            await Client.ConnectAsync();
        }

        private DiscordConfiguration GetClientConfiguration()
        {
            return new DiscordConfiguration
            {
                Token = SettingsLoader.Data.Token,
                TokenType = TokenType.Bot,
                WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew,

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
                IgnoreExtraArguments = true,
                PrefixResolver = CustomPrefixPredicate,

                Services = BuildDependencies()
            };
        }

        private ServiceProvider BuildDependencies()
        {
            return new ServiceCollection()
                .AddSingleton(_oddity)
                .AddSingleton(_cacheService)

                // Embed generators
                .AddScoped<AvatarEmbedGenerator>()
                .AddScoped<LaunchInfoEmbedGenerator>()
                .AddScoped<ChangelogEmbedGenerator>()
                .AddScoped<CompanyInfoEmbedGenerator>()
                .AddScoped<CompanyHistoryTableGenerator>()
                .AddScoped<CompanyHistoryEventEmbedGenerator>()
                .AddScoped<FlickrEmbedGenerator>()
                .AddScoped<RedditEmbedGenerator>()
                .AddScoped<TwitterEmbedGenerator>()
                .AddScoped<LaunchesListTableGenerator>()
                .AddScoped<CoresListTableGenerator>()
                .AddScoped<LaunchpadsEmbedGenerator>()
                .AddScoped<QuoteEmbedGenerator>()
                .AddScoped<RocketsEmbedGenerator>()
                .AddScoped<SubscriptionEmbedGenerator>()
                .AddScoped<UsefulLinksEmbedGenerator>()
                .AddScoped<LaunchNotificationEmbedBuilder>()
                .AddScoped<RoadsterEmbedBuilder>()
                .AddScoped<CoreInfoEmbedGenerator>()

                // Singleton services
                .AddSingleton(new DescriptionService(_cacheService, _oddity))
                .AddSingleton(new UserLaunchSubscriptionsService(_cacheService, new LaunchInfoEmbedGenerator(new TimeZoneService())))
                .AddSingleton(new BotListsService())

                // Normal services
                .AddScoped<ChangelogService>()
                .AddScoped<UsefulLinksService>()
                .AddScoped<FlickrService>()
                .AddScoped<LaunchNotificationsService>()
                .AddScoped<PaginationService>()
                .AddScoped<QuotesService>()
                .AddScoped<RedditService>()
                .AddScoped<SubscriptionsService>()
                .AddScoped<TwitterService>()
                .AddScoped<TimeZoneService>()
                .BuildServiceProvider();
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
                    var genericRegisterCommandMethod = registerCommandsMethod?.MakeGenericMethod(type);
                    genericRegisterCommandMethod?.Invoke(_commands, null);

                    _logger.Info(genericRegisterCommandMethod != null ? $"{type.Name} registered" : $"Can't register {type.Name}");
                }
            }
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info("In Elon We Trust, In Thrust We Trust.");
            _logger.Info($"DSharpPlus {Client.VersionString}");
            //_watchdog.Stop();

            return Task.CompletedTask;
        }

        private Task Client_Heartbeat(HeartbeatEventArgs e)
        {
            _logger.Info($"Heartbeat - ping: {e.Ping} ms");
            return Task.CompletedTask;
        }

        private Task Client_GuildCreated(GuildCreateEventArgs e)
        {
            _logger.Info($"Bot has joined to the new guild, welcome {e.Guild.Name}!");
            return Task.CompletedTask;
        }

        private Task Client_GuildDeleted(GuildDeleteEventArgs e)
        {
            _logger.Info($"Bot has been removed from {e.Guild.Name} guild.");
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            if (e.Exception.InnerException?.Message.Contains("The given key") == false)
            {
                _logger.Error(e.Exception, $"Event Name: {e.EventName}");
            }
            return Task.CompletedTask;
        }

        private Task Client_SocketError(SocketErrorEventArgs e)
        {
            _logger.Warn(e.Exception);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            if (LogExecutedCommands)
            {
                _logger.Info(GetCommandInfo(e.Context));
                await _diagnosticService.AddExecutedCommandAsync(e.Command, e.Context.Guild);
            }
        }

        private async Task Commands_CommandError(CommandErrorEventArgs e)
        {
            var errorEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            var sendErrorMessageOnChannel = true;
            switch (e.Exception)
            {
                case CommandNotFoundException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", "Can't recognize this command, type `e!help` to get full list of them.");
                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));

                    sendErrorMessageOnChannel = false;
                    break;
                }

                case ArgumentException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", $"Invalid parameter, type `e!help {e.Command.Name}` to get more info.");
                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));

                    break;
                }

                case ChecksFailedException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", "You have no permissions to do this action. Remember that some commands (related to subscriptions) requires Manage Messages permission.");
                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));

                    break;
                }

                case UnauthorizedException _:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Error", $"It seems that bot has no required permission to write on channel {e.Context.Channel.Name}.");
                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));

                    await e.Context.Member.SendMessageAsync(embed: errorEmbedBuilder);
                    sendErrorMessageOnChannel = false;

                    break;
                }

                default:
                {
                    errorEmbedBuilder.AddField(":octagonal_sign: Oops", $"Something strange happened when bot was trying to execute `{e.Command.Name}` command. Owner has been notified about this accident and will fix it as soon as possible. Thanks for your patience!");
                    _logger.Error(e.Exception, GetCommandInfo(e.Context));

                    break;
                }
            }

            if (sendErrorMessageOnChannel)
            {
                await e.Context.RespondAsync(string.Empty, false, errorEmbedBuilder);
            }
        }

        private Task Client_SocketClosed(SocketCloseEventArgs e)
        {
            //_watchdog.ResetApp();
            return Task.CompletedTask;
        }

        private void Oddity_OnRequestSend(object sender, RequestSendEventArgs e)
        {
            var message = $"Oddity request sent to {e.Url}";
            if (e.Filters.Any())
            {
                message += $" with filters: {string.Join(", ", e.Filters)}";
            }

            _logger.Info(message);
        }

        private void Oddity_OnResponseReceive(object sender, ResponseReceiveEventArgs e)
        {
            _logger.Info($"Oddity response received ({e.Response?.Length} chars, status {e.StatusCode}).");
        }

        private void Oddity_OnDeserializationError(object sender, ErrorEventArgs e)
        {
            _logger.Warn(e.ErrorContext.Error, "Oddity deserialization error.");
            e.ErrorContext.Handled = true;
        }

        private string GetCommandInfo(CommandContext ctx)
        {
            var infoList = new List<string>
            {
                $"Guild: {ctx.Guild.Name}",
                $"Channel: {ctx.Channel.Name}",
                $"User: {ctx.User.Username}",
                $"Call: {ctx.Message.Content}"
            };

            return string.Join(", ", infoList);
        }
    }
}

