using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Launches")]
    public class LaunchesListCommands
    {
        private OddityCore _oddity;
        private PaginationService _pagination;

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

            _listHeader = new Dictionary<PaginationContentType, string>
            {
                {PaginationContentType.NextLaunches, "List of all upcoming launches:"},
                {PaginationContentType.PastLaunches, "List of all past launches:"},
                {PaginationContentType.AllLaunches, "List of all launches:"}
            };

            Bot.Client.MessageReactionAdded += Client_MessageReactionAdded;
        }

        [Command("upcominglaunches")]
        [Aliases("upcoming", "ul")]
        [Description("Get information about upcoming launches.")]
        public async Task UpcomingLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetUpcoming().ExecuteAsync();
            var launchesList = DisplayLaunchesList(launchData, PaginationContentType.NextLaunches, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _pagination.InitPagination(message, PaginationContentType.NextLaunches);
        }

        [Command("pastlaunches")]
        [Aliases("past", "pl")]
        [Description("Get information about past launches.")]
        public async Task PastLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetPast().ExecuteAsync();
            var launchesList = DisplayLaunchesList(launchData, PaginationContentType.PastLaunches, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _pagination.InitPagination(message, PaginationContentType.PastLaunches);
        }

        [Command("alllaunches")]
        [Aliases("all", "al")]
        [Description("Get information about all launches.")]
        public async Task AllLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().ExecuteAsync();
            var launchesList = DisplayLaunchesList(launchData, PaginationContentType.AllLaunches, 1);

            var message = await ctx.RespondAsync(launchesList);
            await _pagination.InitPagination(message, PaginationContentType.AllLaunches);
        }

        private string DisplayLaunchesList(List<LaunchInfo> launches, PaginationContentType contentType, int currentPage)
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
                var contentType = _pagination.GetContentTypeForMessage(e.Message);
                var previousCurrentPage = _pagination.GetCurrentPage(e.Message);

                List<LaunchInfo> items = null;

                switch (contentType)
                {
                    case PaginationContentType.NextLaunches:
                        items = await _oddity.Launches.GetUpcoming().ExecuteAsync();
                        break;

                    case PaginationContentType.PastLaunches:
                        items = await _oddity.Launches.GetPast().ExecuteAsync();
                        break;

                    case PaginationContentType.AllLaunches:
                        items = await _oddity.Launches.GetAll().ExecuteAsync();
                        break;
                }

                _pagination.DoAction(e.Message, e.Emoji, items.Count);
                var currentPage = _pagination.GetCurrentPage(e.Message);

                if (currentPage != previousCurrentPage)
                {
                    var launchesList = DisplayLaunchesList(items, contentType, currentPage);
                    await e.Message.ModifyAsync(launchesList);
                }

                await e.Message.DeleteReactionAsync(e.Emoji, e.User);
            }
        }
    }
}
