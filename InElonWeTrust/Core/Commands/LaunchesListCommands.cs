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
        private const int _missionNameLength = 25;
        private const int _launchDateLength = 22;
        private const int _siteNameLength = 15;
        private const int _landingLength = 8;

        private int _totalLength => _missionNumberLength + _missionNameLength + _launchDateLength + _siteNameLength + _landingLength;

        public LaunchesListCommands()
        {
            _oddity = new OddityCore();
            _pagination = new PaginationService();

            Bot.Client.MessageReactionAdded += Client_MessageReactionAdded;
        }

        [Command("upcominglaunches")]
        [Aliases("upcoming", "ul")]
        [Description("Get information about upcoming launches.")]
        public async Task UpcomingLaunches(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetUpcoming().ExecuteAsync();
            var launchesList = DisplayLaunchesList(launchData);

            var message = await ctx.RespondAsync(launchesList);
            await _pagination.InitPagination(message, PaginationContentType.NextLaunches);
        }

        private string DisplayLaunchesList(List<LaunchInfo> launches)
        {
            var launchesListBuilder = new StringBuilder();
            launchesListBuilder.Append("```");

            launchesListBuilder.Append($"No. ".PadRight(_missionNumberLength));
            launchesListBuilder.Append("Mission name".PadRight(_missionNameLength));
            launchesListBuilder.Append("Launch date UTC".PadRight(_launchDateLength));
            launchesListBuilder.Append("Launch site".PadRight(_siteNameLength));
            launchesListBuilder.Append("Landing?".PadRight(_landingLength));
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(new string('-', _totalLength));
            launchesListBuilder.Append("\r\n");

            foreach (var launch in launches)
            {
                launchesListBuilder.Append($"{launch.FlightNumber.Value}.".PadRight(_missionNumberLength));
                launchesListBuilder.Append(launch.MissionName.PadRight(_missionNameLength));
                launchesListBuilder.Append(launch.LaunchDateUtc.Value.ToString("G").PadRight(_launchDateLength));
                launchesListBuilder.Append(launch.LaunchSite.SiteName.PadRight(_siteNameLength));

                var landing = launch.Rocket.FirstStage.Cores.Any(p => p?.LandingType != LandingType.Ocean);
                launchesListBuilder.Append((landing ? "yes" : "no").PadRight(_landingLength));
                launchesListBuilder.Append("\r\n");
            }

            launchesListBuilder.Append("```");
            return launchesListBuilder.ToString();
        }

        private async Task Client_MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot && _pagination.IsPaginationSet(e.Message))
            {
                var contentType = _pagination.GetContentTypeForMessage(e.Message);

                List<LaunchInfo> items = null;

                switch (contentType)
                {
                    case PaginationContentType.NextLaunches:
                        items = await _oddity.Launches.GetUpcoming().ExecuteAsync();
                        break;
                }

                _pagination.DoAction(e.Message, e.Emoji, items.Count);

                var currentPage = _pagination.GetCurrentPage(e.Message);
                var maxPagesCount = _pagination.GetPagesCount(items.Count);
                var itemsToDisplay = _pagination.GetItemsToDisplay(items, currentPage);
                var paginationFooter = _pagination.GetPaginationFooter(currentPage, maxPagesCount);

                var launchesList = DisplayLaunchesList(itemsToDisplay);

                await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                await e.Message.ModifyAsync(launchesList);
            }
        }
    }
}
