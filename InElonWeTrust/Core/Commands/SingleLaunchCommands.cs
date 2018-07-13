using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;
using Oddity.API.Models.Launch.Rocket.SecondStage;

namespace InElonWeTrust.Core.Commands
{
    [Commands(":rocket:", "Launches", "Information about all SpaceX launches")]
    public class SingleLaunchCommands
    {
        private OddityCore _oddity;

        public SingleLaunchCommands()
        {
            _oddity = new OddityCore();
        }

        [Command("NextLaunch")]
        [Aliases("Next", "nl")]
        [Description("Get information about the next launch.")]
        public async Task NextLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetNext().ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData);
        }

        [Command("LatestLaunch")]
        [Aliases("Latest", "ll")]
        [Description("Get information about the latest launch.")]
        public async Task LatestLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetLatest().ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData);
        }

        [Command("RandomLaunch")]
        [Aliases("Random", "rl")]
        [Description("Get information about random launch.")]
        public async Task RandomLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData.GetRandomItem());
        }

        [Command("GetLaunch")]
        [Aliases("Launch", "gl")]
        [Description("Get information about launch with the specified flight number (which can be obtained by `e!AllLaunches command`).")]
        public async Task GetLaunch(CommandContext ctx, [Description("Launch number (type `e!AllLaunches to catch them all)")] int id)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().WithFlightNumber(id).ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData.First());
        }

        private async Task DisplayLaunchInfo(CommandContext ctx, LaunchInfo launch)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = launch.Links.MissionPatch ?? Constants.SpaceXLogoImage
            };

            embed.AddField($"{launch.FlightNumber}. {launch.MissionName} ({launch.Rocket.RocketName} {launch.Rocket.RocketType})", launch.Details ?? "*No description at this moment :(*");
            embed.AddField(":clock4: Launch date (UTC):", launch.LaunchDateUtc.Value.ToUniversalTime().ToString("F", CultureInfo.InvariantCulture), true);
            embed.AddField(":stadium: Launchpad:", launch.LaunchSite.SiteName, true);
            embed.AddField($":rocket: First stages ({launch.Rocket.FirstStage.Cores.Count}):", GetCoresData(launch.Rocket.FirstStage.Cores));
            embed.AddField($":package: Payloads ({launch.Rocket.SecondStage.Payloads.Count}):", GetPayloadsData(launch.Rocket.SecondStage.Payloads));
            embed.AddField(":recycle: Reused parts", GetReusedPartsData(launch.Reuse));

            var linksData = GetLinksData(launch);
            if(linksData.Length > 0)
            {
                embed.AddField(":newspaper: Links:", linksData);
            }

            await ctx.RespondAsync(launch.Links.VideoLink, false, embed);
            if(launch.Links.VideoLink != null)
            {
                await ctx.RespondAsync("**YouTube stream:** " + launch.Links.VideoLink);
            }
        }

        private string GetPayloadsData(List<PayloadInfo> payloads)
        {
            var payloadsDataBuilder = new StringBuilder();
            foreach (var payload in payloads)
            {
                payloadsDataBuilder.Append(payload.PayloadId);

                if (payload.PayloadMassKilograms != null)
                {
                    payloadsDataBuilder.Append($" ({payload.PayloadMassKilograms} kg)");
                }

                if (payload.Orbit != null)
                {
                    payloadsDataBuilder.Append($" to {payload.Orbit}");
                }

                payloadsDataBuilder.Append("\r\n");
            }

            if (string.IsNullOrWhiteSpace(payloadsDataBuilder.ToString()))
            {
                payloadsDataBuilder.Clear();
                payloadsDataBuilder.Append("unknown");
            }

            return payloadsDataBuilder.ToString();
        }

        private string GetCoresData(List<CoreInfo> cores)
        {
            var coresDataBuilder = new StringBuilder();
            foreach (var core in cores)
            {
                coresDataBuilder.Append($"{core.CoreSerial}");
                if (core.Block != null)
                {
                    coresDataBuilder.Append($" (block {core.Block})");
                }

                if (core.LandingType != null && core.LandingType != LandingType.Ocean)
                {
                    coresDataBuilder.Append($", landing on {core.LandingVehicle}");
                }

                if (core.LandSuccess != null)
                {
                    coresDataBuilder.Append($" ({(core.LandSuccess.Value ? "success" : "fail")})");
                }

                coresDataBuilder.Append("\r\n");
            }

            if (string.IsNullOrWhiteSpace(coresDataBuilder.ToString()))
            {
                coresDataBuilder.Clear();
                coresDataBuilder.Append("unknown");
            }

            return coresDataBuilder.ToString();
        }

        private string GetLinksData(LaunchInfo info)
        {
            var links = new List<string>();

            if (info.Links.RedditLaunch != null)
            {
                links.Add($"[Reddit]({info.Links.RedditLaunch})");
            }

            if (info.Links.Presskit != null)
            {
                links.Add($"[Presskit]({info.Links.Presskit})");
            }

            if (info.Telemetry.FlightClub != null)
            {
                links.Add($"[Telemetry]({info.Telemetry.FlightClub})");
            }

            if (info.Links.VideoLink != null)
            {
                links.Add($"[YouTube stream]({info.Links.VideoLink})");
            }

            return string.Join(", ", links);
        }

        private string GetReusedPartsData(ReuseInfo reused)
        {
            var reusedPartsList = new List<string>();

            if (reused.Core.HasValue && reused.Core.Value)
            {
                reusedPartsList.Add("Core");
            }

            if (reused.Capsule.HasValue && reused.Capsule.Value)
            {
                reusedPartsList.Add("Capsule");
            }

            if (reused.Fairings.HasValue && reused.Fairings.Value)
            {
                reusedPartsList.Add("Fairings");
            }

            if (reused.FirstSideCore.HasValue && reused.FirstSideCore.Value)
            {
                reusedPartsList.Add("First side core");
            }

            if (reused.SecondSideCore.HasValue && reused.SecondSideCore.Value)
            {
                reusedPartsList.Add("Second side core");
            }

            return reusedPartsList.Count > 0 ? string.Join(", ", reusedPartsList) : "none";
        }
    }
}
