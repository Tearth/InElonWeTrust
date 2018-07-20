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
    [Commands(":rocket:", "Launches", "Information about all SpaceX launches")]
    public class LaunchesListCommands
    {
        private OddityCore _oddity;
        private PaginationService _paginationService;
        private CacheService _cacheService;

        private const int _missionNumberLength = 4;
        private const int _missionNameLength = 23;
        private const int _launchDateLength = 21;
        private const int _siteNameLength = 18;
        private const int _landingLength = 7;

        private int _totalLength => _missionNumberLength + _missionNameLength + _launchDateLength + _siteNameLength + _landingLength;

        private Dictionary<CacheContentType, string> _listHeader;

        public LaunchesListCommands(OddityCore oddity, PaginationService paginationService, CacheService cacheService)
        {
            _oddity = oddity;
            _paginationService = paginationService;
            _cacheService = cacheService;

            _listHeader = new Dictionary<CacheContentType, string>
            {
                {CacheContentType.UpcomingLaunches, "List of all upcoming launches:"},
                {CacheContentType.PastLaunches, "List of all past launches:"},
                {CacheContentType.AllLaunches, "List of all launches:"},
                {CacheContentType.FailedStarts, "List of all failed starts:"},
                {CacheContentType.FailedLandings, "List of all failed landings:"},
                {CacheContentType.LaunchesWithOrbit, "List of launches with the specified orbit:"}
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
        [Aliases("All", "al")]
        [Description("Get information about all launches.")]
        public async Task AllLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await DisplayLaunches(ctx, CacheContentType.AllLaunches);
        }

        [Command("FailedStarts")]
        [Aliases("fs")]
        [Description("Get information about all failed launches.")]
        public async Task FailedStarts(CommandContext ctx)
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

            var launchesList = GetLaunchesTable(launchData, contentType, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _paginationService.InitPagination(message, contentType, parameter);
        }

        private async Task<List<LaunchInfo>> GetLaunches(CacheContentType contentType, string parameter = null)
        {
            return await _cacheService.Get<List<LaunchInfo>>(contentType, parameter);
        }

        private string GetLaunchesTable(List<LaunchInfo> launches, CacheContentType contentType, int currentPage)
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

            var itemsToDisplay = _paginationService.GetItemsToDisplay(launches, currentPage);
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

            var maxPagesCount = _paginationService.GetPagesCount(launches.Count);
            var paginationFooter = _paginationService.GetPaginationFooter(currentPage, maxPagesCount);

            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("Type e!getlaunch <number> to get more information.");
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(paginationFooter);
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("```");

            return launchesListBuilder.ToString();
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
            if (!e.User.IsBot && _paginationService.IsPaginationSet(e.Message))
            {
                var paginationData = _paginationService.GetPaginationDataForMessage(e.Message);
                List<LaunchInfo> items = null;

                // TODO: temp workaround, change to something more professional
                try
                {
                    items = await GetLaunches(paginationData.ContentType, paginationData.Parameter);
                }
                catch
                {
                    return;
                }

                if (items != null)
                {
                    if (_paginationService.DoAction(e.Message, e.Emoji, items.Count))
                    {
                        var updatedPaginationData = _paginationService.GetPaginationDataForMessage(e.Message);
                        var launchesList = GetLaunchesTable(items, updatedPaginationData.ContentType, updatedPaginationData.CurrentPage);
                        await e.Message.ModifyAsync(launchesList);
                    }

                    await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                }
            }
        }
    }
}
