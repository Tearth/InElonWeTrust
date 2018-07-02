using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Launches")]
    public class LaunchesListCommands
    {
        private OddityCore _oddity;

        private const int _missionNumberLength = 4;
        private const int _missionNameLength = 25;
        private const int _launchDateLength = 22;
        private const int _siteNameLength = 15;
        private const int _landingLength = 8;

        private int _totalLength => _missionNumberLength + _missionNameLength + _launchDateLength + _siteNameLength + _landingLength;

        public LaunchesListCommands()
        {
            _oddity = new OddityCore();
        }

        [Command("upcominglaunches")]
        [Aliases("upcoming", "ul")]
        [Description("Get information about upcoming launches.")]
        public async Task NextLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetUpcoming().ExecuteAsync();
            await DisplayLaunchesList(ctx, launchData);
        }

        private async Task DisplayLaunchesList(CommandContext ctx, List<LaunchInfo> launches)
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

            var launchesToDisplay = launches.Take(10);
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

            var message = await ctx.RespondAsync(launchesListBuilder.ToString());
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, PaginationManager.LeftEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, PaginationManager.RightEmojiName));
        }
    }
}
