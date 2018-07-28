using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.TableGenerators;
using NLog;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.SecondStage.Orbit;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Launches)]
    public class LaunchesListCommands
    {
        private readonly OddityCore _oddity;
        private readonly PaginationService _paginationService;
        private readonly CacheService _cacheService;
        private readonly LaunchesListTableGenerator _launchesListTableGenerator;

        private readonly List<CacheContentType> _allowedPaginationTypes;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LaunchesListCommands(OddityCore oddity, PaginationService paginationService, CacheService cacheService, LaunchesListTableGenerator launchesListTableGenerator)
        {
            _oddity = oddity;
            _paginationService = paginationService;
            _cacheService = cacheService;
            _launchesListTableGenerator = launchesListTableGenerator;

            _allowedPaginationTypes = new List<CacheContentType>
            {
                CacheContentType.UpcomingLaunches,
                CacheContentType.PastLaunches,
                CacheContentType.AllLaunches,
                CacheContentType.FailedStarts,
                CacheContentType.FailedLandings,
                CacheContentType.LaunchesWithOrbit
            };

            Bot.Client.MessageReactionAdded += Client_MessageReactionAdded;

            _cacheService.RegisterDataProvider(CacheContentType.UpcomingLaunches, async p => await _oddity.Launches.GetUpcoming().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.PastLaunches, async p => await _oddity.Launches.GetPast().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.AllLaunches, async p => await _oddity.Launches.GetAll().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.FailedStarts, async p => await _oddity.Launches.GetAll().WithLaunchSuccess(false).ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.FailedLandings, async p => await _oddity.Launches.GetAll().WithLandSuccess(false).ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.LaunchesWithOrbit, async p => await GetLaunchesWithOrbitDataProvider(p));
        }

        [Command("UpcomingLaunches")]
        [Aliases("Upcoming", "ul")]
        [Description("Get information about upcoming launches.")]
        public async Task UpcomingLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.UpcomingLaunches);
        }

        [Command("PastLaunches")]
        [Aliases("Past", "pl")]
        [Description("Get information about past launches.")]
        public async Task PastLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.PastLaunches);
        }

        [Command("AllLaunches")]
        [Aliases("All", "Launches", "al")]
        [Description("Get information about all launches.")]
        public async Task AllLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.AllLaunches);
        }

        [Command("FailedLaunches")]
        [Aliases("FailedStarts", "fs")]
        [Description("Get information about all failed launches.")]
        public async Task FailedLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.FailedStarts);
        }

        [Command("FailedLandings")]
        [Aliases("fl")]
        [Description("Get information about all failed launches.")]
        public async Task FailedLandings(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.FailedLandings);
        }

        [Command("LaunchesWithOrbit")]
        [Aliases("Orbit", "o")]
        [Description("Get information about all launches with the specified orbit.")]
        public async Task LaunchesWithOrbit(CommandContext ctx, [Description("Available orbits: ESL1, GTO, HCO, HEO, ISS, LEO, PO, SSO")] string orbitType)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.LaunchesWithOrbit, orbitType);
        }

        private string BuildTableWithPagination(List<LaunchInfo> launches, CacheContentType contentType, int currentPage)
        {
            var itemsToDisplay = _paginationService.GetItemsToDisplay(launches, currentPage);
            itemsToDisplay = itemsToDisplay.OrderBy(p => p.LaunchDateUtc.Value).ToList();

            var maxPagesCount = _paginationService.GetPagesCount(launches.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _launchesListTableGenerator.Build(itemsToDisplay, contentType, currentPage, paginationFooter);
        }

        private async Task DisplayLaunches(CommandContext ctx, CacheContentType contentType, string parameter = null)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await GetLaunches(contentType, parameter);
            if (launchData == null)
            {
                var embed = new DiscordEmbedBuilder {Color = new DiscordColor(Constants.EmbedErrorColor)};
                embed.AddField("Error", $"Invalid parameter, type `e!help {ctx.Command.Name}` to get more information.");

                await ctx.RespondAsync("", false, embed);
                return;
            }

            var launchesList = BuildTableWithPagination(launchData, contentType, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _paginationService.InitPagination(message, contentType, parameter);
        }

        private async Task<List<LaunchInfo>> GetLaunches(CacheContentType contentType, string parameter = null)
        {
            return await _cacheService.Get<List<LaunchInfo>>(contentType, parameter);
        }

        private async Task<object> GetLaunchesWithOrbitDataProvider(string parameter)
        {
            if (Enum.TryParse(typeof(OrbitType), parameter, true, out var output))
            {
                return await _oddity.Launches.GetAll().WithOrbit((OrbitType)output).ExecuteAsync();
            }

            return null;
        }

        private async Task Client_MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || !_paginationService.IsPaginationSet(e.Message))
            {
                return;
            }

            var paginationData = _paginationService.GetPaginationDataForMessage(e.Message);
            if (_allowedPaginationTypes.Contains(paginationData.ContentType))
            {
                var items = await GetLaunches(paginationData.ContentType, paginationData.Parameter);
                if (items != null)
                {
                    var editedMessage = e.Message;

                    if (_paginationService.DoAction(e.Message, e.Emoji, items.Count))
                    {
                        var updatedPaginationData = _paginationService.GetPaginationDataForMessage(e.Message);
                        var launchesList = BuildTableWithPagination(items, updatedPaginationData.ContentType,
                            updatedPaginationData.CurrentPage);
                        editedMessage = await e.Message.ModifyAsync(launchesList);
                    }

                    try
                    {
                        await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                    }
                    catch (UnauthorizedException)
                    {
                        var oldMessageContent = editedMessage.Content ??
                                                (await e.Channel.GetMessageAsync(e.Message.Id)).Content;

                        if (oldMessageContent.EndsWith("```", StringComparison.InvariantCultureIgnoreCase))
                        {
                            oldMessageContent += "\r\n";
                            oldMessageContent +=
                                "*It seems that I have no enough permissions to do pagination properly. Please check " +
                                "bot/channel permissions and be sure that I have ability to manage messages.*";
                        }

                        _logger.Warn("Can't do pagination due to permissions.");
                        await editedMessage.ModifyAsync(oldMessageContent);
                    }
                }
            }
        }
    }
}
