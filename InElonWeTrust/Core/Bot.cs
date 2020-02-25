using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Commands.Attributes;
using InElonWeTrust.Core.EmbedGenerators;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Formatters;
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
using StringComparer = InElonWeTrust.Core.Helpers.Comparers.StringComparer;

namespace InElonWeTrust.Core
{
    public class Bot
    {
        public static DiscordClient Client { get; set; }
        public static bool LogExecutedCommands { get; set; }
        public static int HandledMessagesCount { get; set; }
        public CacheService CacheService { get; set; }

        private readonly CommandsNextExtension _commands;
        private readonly OddityCore _oddity;
        private readonly DiagnosticService _diagnosticService;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Bot()
        {
            _oddity = new OddityCore();
            _diagnosticService = new DiagnosticService();

            _oddity.SetTimeout(2);
            _oddity.OnRequestSend += Oddity_OnRequestSend;
            _oddity.OnResponseReceive += Oddity_OnResponseReceive;
            _oddity.OnDeserializationError += Oddity_OnDeserializationError;

            Client = new DiscordClient(GetClientConfiguration());
            CacheService = new CacheService();
            LogExecutedCommands = true;
            
            Client.Ready += Client_Ready;
            Client.Heartbeated += Client_Heartbeat;
            Client.GuildCreated += Client_GuildCreated;
            Client.GuildDeleted += Client_GuildDeleted;
            Client.ClientErrored += Client_ClientError;
            Client.SocketErrored += Client_SocketError;
            Client.SocketClosed += Client_SocketClosed;
            Client.Resumed += Client_Resumed;
            Client.MessageCreated += Client_MessageCreated;

            _commands = Client.UseCommandsNext(GetCommandsConfiguration());
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandError;
            _commands.SetHelpFormatter<CustomHelpFormatter>();

            RegisterCommands();
        }

        public async Task Run()
        {
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
                IgnoreExtraArguments = true,
                PrefixResolver = CustomPrefixPredicate,

                Services = BuildDependencies()
            };
        }

        private ServiceProvider BuildDependencies()
        {
            return new ServiceCollection()
                .AddSingleton(_oddity)
                .AddSingleton(CacheService)

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
                .AddScoped<UptimeEmbedGenerator>()
                .AddScoped<PingEmbedGenerator>()
                .AddScoped<TimeZoneEmbedGenerator>()

                // Singleton services
                .AddSingleton(new DescriptionService(CacheService, _oddity))
                .AddSingleton(new UserLaunchSubscriptionsService(CacheService, new LaunchInfoEmbedGenerator(new TimeZoneService())))
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
                if (attributes.Any(p => p.GetType() == typeof(CommandsGroupAttribute)))
                {
                    var genericRegisterCommandMethod = registerCommandsMethod?.MakeGenericMethod(type);
                    genericRegisterCommandMethod?.Invoke(_commands, null);

                    if (genericRegisterCommandMethod == null)
                    {
                        _logger.Fatal($"Can't register {type.Name}");
                        return;
                    }

                    _logger.Info($"{type.Name} registered");
                }
            }
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Info("In Elon We Trust, In Thrust We Trust");
            _logger.Info($"DSharpPlus v{Client.VersionString}");
            _logger.Info($"Gateway v{Client.GatewayVersion} ({Client.GatewayUri})");
            _logger.Info(GetOsInfo());
            _logger.Info(GetDotNetCoreInfo());

            return Task.CompletedTask;
        }

        private Task Client_Heartbeat(HeartbeatEventArgs e)
        {
            _logger.Info($"Heartbeat - ping: {e.Ping} ms");
            return Task.CompletedTask;
        }

        private Task Client_GuildCreated(GuildCreateEventArgs e)
        {
            _logger.Info($"$$$$ Bot has joined to the new guild, welcome {e.Guild.Name} [{e.Guild.Id}] " +
                         $"({e.Guild.MemberCount} members) $$$$");
            return Task.CompletedTask;
        }

        private Task Client_GuildDeleted(GuildDeleteEventArgs e)
        {
            _logger.Info($"Bot has been removed from {e.Guild.Name} [{e.Guild.Id}] ({e.Guild.MemberCount} members)");
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            //if (e.Exception.InnerException?.Message.Contains("The given key") == false)
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
                case CommandNotFoundException commandNotFoundException:
                {
                    if (commandNotFoundException.CommandName == null)
                    {
                        return;
                    }

                    var messageContent = e.Context.Message.Content;
                    var messageWithoutPrefix = messageContent.Replace(e.Context.Prefix, "");
                    var splitMessage = messageWithoutPrefix.Split(' ');
                    var commandName = splitMessage[0].ToLower();
                    var parameters = string.Join(' ', splitMessage.Skip(1));

                    var similarCommand = commandName == "help" ? null :
                        _commands.RegisteredCommands
                        .Select(p => new
                        {
                            Name = p.Key,
                            Command = p.Value,
                            Distance = StringComparer.CalculateLevenshteinDistance(
                                 p.Key.ToLower(),
                                 (commandNotFoundException.CommandName ?? string.Empty).ToLower())
                        })
                        .Where(p => 
                            !p.Command.IsHidden &&
                            p.Distance <= (commandNotFoundException.CommandName ?? string.Empty).Length / 10 + 1)
                        .OrderBy(p => p.Distance)
                        .FirstOrDefault();

                    if (similarCommand != null)
                    {
                        var newMessage = $"{similarCommand.Command.Name} {parameters}";

                        var message = $"Typo in command detected but found a similar one: " +
                                      $"{commandNotFoundException.CommandName} -> " +
                                      $"{similarCommand.Name}";

                        _logger.Warn(message);
                        await e.Context.RespondAsync($"*{message}*");

                        var fakeContext = e.Context.CommandsNext.CreateFakeContext(e.Context.User, e.Context.Channel, newMessage, e.Context.Prefix, similarCommand.Command, parameters);
                        await e.Context.CommandsNext.ExecuteCommandAsync(fakeContext);
                    }
                    else
                    {
                        errorEmbedBuilder.Title = ":octagonal_sign: Error";
                        errorEmbedBuilder.Description = "Can't recognize this command, type `e!help` to get full list of them.";

                        _logger.Warn(e.Exception, GetCommandInfo(e.Context));
                    }

                    sendErrorMessageOnChannel = false;
                    break;
                }

                case ArgumentException _:
                {
                    errorEmbedBuilder.Title = ":octagonal_sign: Error";
                    errorEmbedBuilder.Description = $"Invalid parameter, type `e!help {e.Command.Name}` to get more info.";

                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));
                    break;
                }

                case ChecksFailedException _:
                {
                    errorEmbedBuilder.Title = ":octagonal_sign: Error";
                    errorEmbedBuilder.Description = "You have no permissions to do this action. Remember that some commands " +
                                                    "(related to notifications) require \"Manage Messages\" permission.";

                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));
                    break;
                }

                case UnauthorizedException _:
                {
                    errorEmbedBuilder.Title = ":octagonal_sign: Error";
                    errorEmbedBuilder.Description = $"It seems that bot has no required permission to write on channel {e.Context.Channel.Name}.";

                    _logger.Warn(e.Exception, GetCommandInfo(e.Context));

                    await e.Context.Member.SendMessageAsync(embed: errorEmbedBuilder);
                    sendErrorMessageOnChannel = false;

                    break;
                }

                default:
                {
                    errorEmbedBuilder.Title = ":octagonal_sign: Oops";
                    errorEmbedBuilder.Description = $"Something strange happened when bot was trying to execute `{e.Command.Name}` command. " +
                                                    $"Owner has been notified about this accident and will fix it as soon as possible. " +
                                                    $"Thanks for your patience!";

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
            _logger.Warn($"Client socket error. Message: {e.CloseMessage} ({e.CloseCode})");
            return Task.CompletedTask;
        }

        private Task Client_Resumed(ReadyEventArgs e)
        {
            _logger.Warn("Client resumed");
            return Task.CompletedTask;
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            HandledMessagesCount++;

            if (SettingsLoader.Data.Prefixes.Contains(e.Message.Content))
            {
                var helpCommand = _commands.RegisteredCommands.First(p => p.Key == "help").Value;
                var fakeContext = _commands.CreateFakeContext(e.Author, e.Channel, e.Message.Content, "help", helpCommand);
                await _commands.ExecuteCommandAsync(fakeContext);
            }
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
            _logger.Info($"Oddity response received ({e.Response?.Length} chars, status {e.StatusCode})");
        }

        private void Oddity_OnDeserializationError(object sender, ErrorEventArgs e)
        {
            _logger.Warn(e.ErrorContext.Error, "Oddity deserialization error");
            e.ErrorContext.Handled = true;
        }

        private string GetCommandInfo(CommandContext ctx)
        {
            var infoList = new List<string>
            {
                $"Guild: {ctx.Guild.Name} [{ctx.Guild.Id}]",
                $"Channel: {ctx.Channel.Name} [{ctx.Channel.Id}]",
                $"User: {ctx.User.Username} [{ctx.User.Id}]",
                $"Call: {ctx.Message.Content}"
            };

            return string.Join(", ", infoList);
        }

        private string GetOsInfo()
        {
            return System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim();
        }

        private string GetDotNetCoreInfo()
        {
            return Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName.Trim();
        }
    }
}

