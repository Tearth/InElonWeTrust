using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;
using Oddity.API.Models.Launch.Rocket.SecondStage.Orbit;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Launches")]
    public class LaunchesListCommands
    {
        private OddityCore _oddity;
        private PaginationService _pagination;
        private CacheService<PaginationContentType, List<LaunchInfo>> _cacheService;

        private const int _missionNumberLength = 4;
        private const int _missionNameLength = 23;
        private const int _launchDateLength = 21;
        private const int _siteNameLength = 18;
        private const int _landingLength = 7;

        private int _totalLength => _missionNumberLength + _missionNameLength + _launchDateLength + _siteNameLength + _landingLength;

        private Dictionary<PaginationContentType, string> _listHeader;

        public LaunchesListCommands()
        {
            _oddity = new OddityCore();
            _pagination = new PaginationService();
            _cacheService = new CacheService<PaginationContentType, List<LaunchInfo>>();

            _listHeader = new Dictionary<PaginationContentType, string>
            {
                {PaginationContentType.UpcomingLaunches, "List of all upcoming launches:"},
                {PaginationContentType.PastLaunches, "List of all past launches:"},
                {PaginationContentType.AllLaunches, "List of all launches:"},
                {PaginationContentType.FailedStarts, "List of all failed starts:"},
                {PaginationContentType.FailedLandings, "List of all failed landings:"},
                {PaginationContentType.LaunchesWithOrbit, "List of launches with the specified orbit:"}
            };

            Bot.Client.MessageReactionAdded += Client_MessageReactionAdded;
        }

        [Command("UpcomingLaunches")]
        [Aliases("Upcoming", "ul")]
        [Description("Get information about upcoming launches.")]
        public async Task UpcomingLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, PaginationContentType.UpcomingLaunches);
        }

        [Command("PastLaunches")]
        [Aliases("Past", "pl")]
        [Description("Get information about past launches.")]
        public async Task PastLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, PaginationContentType.PastLaunches);
        }

        [Command("AllLaunches")]
        [Aliases("All", "al")]
        [Description("Get information about all launches.")]
        public async Task AllLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, PaginationContentType.AllLaunches);
        }

        [Command("FailedStarts")]
        [Aliases("fs")]
        [Description("Get information about all failed launches.")]
        public async Task FailedStarts(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, PaginationContentType.FailedStarts);
        }

        [Command("FailedLandings")]
        [Aliases("fl")]
        [Description("Get information about all failed launches.")]
        public async Task FailedLandings(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, PaginationContentType.FailedLandings);
        }

        [Command("LaunchesWithOrbit")]
        [Aliases("Orbit", "o")]
        [Description("Get information about all launches with the specified orbit.")]
        public async Task LaunchesWithOrbit(CommandContext ctx, [Description("Available orbits: ESL1, GTO, HCO, HEO, ISS, LEO, PO, SSO")] string orbitType)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, PaginationContentType.LaunchesWithOrbit, orbitType);
        }

        private async Task DisplayLaunches(CommandContext ctx, PaginationContentType contentType, string parameter = null)
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

            var launchesList = GetLaunchesTable(launchData, contentType, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _pagination.InitPagination(message, contentType, parameter);
        }

        private async Task<List<LaunchInfo>> GetLaunches(PaginationContentType contentType, string parameter = null)
        {
            Func<Task<List<LaunchInfo>>> dataProviderDelegate = null;

            switch (contentType)
            {
                case PaginationContentType.UpcomingLaunches:
                    dataProviderDelegate = async () => await _oddity.Launches.GetUpcoming().ExecuteAsync();
                    break;

                case PaginationContentType.PastLaunches:
                    dataProviderDelegate = async () => await _oddity.Launches.GetPast().ExecuteAsync();
                    break;

                case PaginationContentType.AllLaunches:
                    dataProviderDelegate = async () => await _oddity.Launches.GetAll().ExecuteAsync();
                    break;

                case PaginationContentType.FailedStarts:
                    dataProviderDelegate = async () => await _oddity.Launches.GetAll().WithLaunchSuccess(false).ExecuteAsync();
                    break;

                case PaginationContentType.FailedLandings:
                    dataProviderDelegate = async () => await _oddity.Launches.GetAll().WithLandSuccess(false).ExecuteAsync();
                    break;

                case PaginationContentType.LaunchesWithOrbit:
                    if (!Enum.TryParse(typeof(OrbitType), parameter, true, out var orbitType))
                    {
                        return null;
                    }

                    dataProviderDelegate = async () => await _oddity.Launches.GetAll().WithOrbit((OrbitType)orbitType).ExecuteAsync();
                    break;
            }

            return await _cacheService.GetAndUpdateAsync(contentType, dataProviderDelegate);
        }

        private string GetLaunchesTable(List<LaunchInfo> launches, PaginationContentType contentType, int currentPage)
        {
            var launchesListBuilder = new StringBuilder();
            launchesListBuilder.Append($":rocket:  **{_listHeader[contentType]}**");
            launchesListBuilder.Append("\r\n");

            launchesListBuilder.Append("```");

            launchesListBuilder.Append("No. ".PadRight(_missionNumberLength));
            launchesListBuilder.Append("Mission name".PadRight(_missionNameLength));
            launchesListBuilder.Append("Launch date UTC".PadRight(_launchDateLength));
            launchesListBuilder.Append("Launch site".PadRight(_siteNameLength));
            launchesListBuilder.Append("Landing".PadRight(_landingLength));
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(new string('-', _totalLength));
            launchesListBuilder.Append("\r\n");

            var itemsToDisplay = _pagination.GetItemsToDisplay(launches, currentPage);
            foreach (var launch in itemsToDisplay)
            {
                launchesListBuilder.Append($"{launch.FlightNumber.Value}.".PadRight(_missionNumberLength));
                launchesListBuilder.Append(launch.MissionName.ShortenString(_missionNameLength - 5).PadRight(_missionNameLength));
                launchesListBuilder.Append(launch.LaunchDateUtc.Value.ToString("G").PadRight(_launchDateLength));
                launchesListBuilder.Append(launch.LaunchSite.SiteName.PadRight(_siteNameLength));

                var landing = launch.Rocket.FirstStage.Cores.Any(p => p.LandingType != null && p.LandingType != LandingType.Ocean);
                launchesListBuilder.Append((landing ? "yes" : "no").PadRight(_landingLength));
                launchesListBuilder.Append("\r\n");
            }

            var maxPagesCount = _pagination.GetPagesCount(launches.Count);
            var paginationFooter = _pagination.GetPaginationFooter(currentPage, maxPagesCount);

            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(paginationFooter);
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("```");

            return launchesListBuilder.ToString();
        }

        private async Task Client_MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot && _pagination.IsPaginationSet(e.Message))
            {
                var paginationData = _pagination.GetPaginationDataForMessage(e.Message);
                var items = await GetLaunches(paginationData.ContentType);

                if (_pagination.DoAction(e.Message, e.Emoji, items.Count))
                {
                    var updatedPaginationData = _pagination.GetPaginationDataForMessage(e.Message);
                    var launchesList = GetLaunchesTable(items, updatedPaginationData.ContentType, updatedPaginationData.CurrentPage);
                    await e.Message.ModifyAsync(launchesList);
                }

                await e.Message.DeleteReactionAsync(e.Emoji, e.User);
            }
        }
    }
}
