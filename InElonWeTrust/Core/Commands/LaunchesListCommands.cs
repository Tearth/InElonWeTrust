using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using InElonWeTrust.Core.TableGenerators;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.SecondStage.Orbit;

namespace InElonWeTrust.Core.Commands
{
    [CommandsGroup(GroupType.Launches)]
    public class LaunchesListCommands : BaseCommandModule
    {
        private readonly OddityCore _oddity;
        private readonly PaginationService _paginationService;
        private readonly CacheService _cacheService;
        private readonly LaunchesListTableGenerator _launchesListTableGenerator;

        private readonly List<CacheContentType> _allowedPaginationTypes;

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

            Bot.Client.MessageReactionAdded += Client_MessageReactionAddedAsync;

            _cacheService.RegisterDataProvider(CacheContentType.UpcomingLaunches, async p => await _oddity.Launches.GetUpcoming().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.PastLaunches, async p => await _oddity.Launches.GetPast().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.AllLaunches, async p => await _oddity.Launches.GetAll().ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.FailedStarts, async p => await _oddity.Launches.GetAll().WithLaunchSuccess(false).ExecuteAsync());
            _cacheService.RegisterDataProvider(CacheContentType.LaunchesWithOrbit, async p => await GetLaunchesWithOrbitDataProviderAsync(p));
        }

        [Command("UpcomingLaunches"), Aliases("Upcoming", "NextLaunches")]
        [Description("Get a list of upcoming launches.")]
        public async Task UpcomingLaunchesAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunchesAsync(ctx, CacheContentType.UpcomingLaunches);
        }

        [Command("PastLaunches"), Aliases("Past", "PreviousLaunches")]
        [Description("Get a list of past launches.")]
        public async Task PastLaunchesAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunchesAsync(ctx, CacheContentType.PastLaunches);
        }

        [Command("AllLaunches"), Aliases("All", "Launches")]
        [Description("Get a list of all launches.")]
        public async Task AllLaunchesAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunchesAsync(ctx, CacheContentType.AllLaunches);
        }

        [Command("FailedLaunches")]
        [Description("Get a list of all failed launches.")]
        public async Task FailedLaunchesAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunchesAsync(ctx, CacheContentType.FailedStarts);
        }

        [Command("LaunchesWithOrbit")]
        [Description("Get a list of all launches with the specified orbit.")]
        public async Task LaunchesWithOrbitAsync(CommandContext ctx, [Description("Available orbits: PO, LEO, VLEO, MEO, ISS, GTO, SSO, HCO, HEO, SO, ESL1")] string orbitType)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunchesAsync(ctx, CacheContentType.LaunchesWithOrbit, orbitType);
        }

        private string BuildTableWithPagination(List<LaunchInfo> launches, CacheContentType contentType, int currentPage)
        {
            var itemsToDisplay = _paginationService.GetItemsToDisplay(launches, currentPage);
            itemsToDisplay = itemsToDisplay.OrderBy(p => p.LaunchDateUtc ?? DateTime.MinValue).ToList();

            var maxPagesCount = _paginationService.GetPagesCount(launches.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            return _launchesListTableGenerator.Build(itemsToDisplay, contentType, currentPage, paginationFooter);
        }

        private async Task DisplayLaunchesAsync(CommandContext ctx, CacheContentType contentType, string parameter = null)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await GetLaunchesAsync(contentType, parameter);
            if (launchData == null)
            {
                var embed = new DiscordEmbedBuilder {Color = new DiscordColor(Constants.EmbedErrorColor)};
                embed.AddField("Error", $"Invalid parameter, type `e!help {ctx.Command.Name}` to get more information.");

                await ctx.RespondAsync(string.Empty, false, embed);
                return;
            }

            var launchesList = BuildTableWithPagination(launchData, contentType, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _paginationService.InitPaginationAsync(message, contentType, parameter);
        }

        private async Task<List<LaunchInfo>> GetLaunchesAsync(CacheContentType contentType, string parameter = null)
        {
            return await _cacheService.GetAsync<List<LaunchInfo>>(contentType, parameter);
        }

        private async Task<object> GetLaunchesWithOrbitDataProviderAsync(string parameter)
        {
            if (Enum.TryParse(typeof(OrbitType), parameter, true, out var output))
            {
                return await _oddity.Launches.GetAll().WithOrbit((OrbitType)output).ExecuteAsync();
            }

            return null;
        }

        private async Task Client_MessageReactionAddedAsync(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || !await _paginationService.IsPaginationSetAsync(e.Message))
            {
                return;
            }

            var paginationData = await _paginationService.GetPaginationDataForMessageAsync(e.Message);
            if (_allowedPaginationTypes.Contains(paginationData.ContentType))
            {
                var items = await GetLaunchesAsync(paginationData.ContentType, paginationData.Parameter);
                if (items != null)
                {
                    var editedMessage = e.Message;

                    if (await _paginationService.DoActionAsync(editedMessage, e.Emoji, items.Count))
                    {
                        var updatedPaginationData = await _paginationService.GetPaginationDataForMessageAsync(editedMessage);
                        var launchesList = BuildTableWithPagination(items, updatedPaginationData.ContentType, updatedPaginationData.CurrentPage);

                        editedMessage = await editedMessage.ModifyAsync(launchesList);
                    }

                    await _paginationService.DeleteReactionAsync(editedMessage, e.User, e.Emoji);
                }
            }
        }
    }
}
