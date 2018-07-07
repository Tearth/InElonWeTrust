using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.LinkShortener;
using Oddity;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;
using Oddity.API.Models.Launch.Rocket.SecondStage;

namespace InElonWeTrust.Core.Commands
{
    [Commands("Launches")]
    public class SingleLaunchCommands
    {
        private OddityCore _oddity;
        private LinkShortenerService _linkShortenerService;

        public SingleLaunchCommands()
        {
            _oddity = new OddityCore();
            _linkShortenerService = new LinkShortenerService();
        }

        [Command("nextlaunch")]
        [Aliases("next", "nl")]
        [Description("Get information about the next launch.")]
        public async Task NextLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetNext().ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData);
        }

        [Command("latestlaunch")]
        [Aliases("latest", "ll")]
        [Description("Get information about the latest launch.")]
        public async Task LatestLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetLatest().ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData);
        }

        [Command("randomlaunch")]
        [Aliases("random", "rl")]
        [Description("Get information about random launch.")]
        public async Task RandomLaunch(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchData = await _oddity.Launches.GetAll().ExecuteAsync();
            await DisplayLaunchInfo(ctx, launchData.GetRandomItem());
        }

        [Command("getlaunch")]
        [Aliases("getl", "gl")]
        [Description("Get information about launch with the specified flight number.")]
        public async Task GetLaunch(CommandContext ctx, int id)
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
            embed.AddField(":clock4: Launch date:", launch.LaunchDateUtc.Value.ToUniversalTime().ToString("F"), true);
            embed.AddField(":stadium: Launchpad:", launch.LaunchSite.SiteName, true);
            embed.AddField($":rocket: First stages ({launch.Rocket.FirstStage.Cores.Count}):", GetCoresData(launch.Rocket.FirstStage.Cores));
            embed.AddField($":package: Payloads ({launch.Rocket.SecondStage.Payloads.Count}):", GetPayloadsData(launch.Rocket.SecondStage.Payloads));
            embed.AddField(":recycle: Reused parts", GetReusedPartsData(launch.Reuse));

            var linksData = await GetLinksData(launch);
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

            return coresDataBuilder.ToString();
        }

        private async Task<string> GetLinksData(LaunchInfo info)
        {
            var linksDataBuilder = new StringBuilder();

            if (info.Links.RedditLaunch != null)
            {
                linksDataBuilder.Append($"Reddit: {await _linkShortenerService.GetShortcutLinkAsync(info.Links.RedditLaunch)}\r\n");
            }

            if (info.Links.Presskit != null)
            {
                linksDataBuilder.Append($"Presskit: {await _linkShortenerService.GetShortcutLinkAsync(info.Links.Presskit)}\r\n");
            }

            if (info.Telemetry.FlightClub != null)
            {
                linksDataBuilder.Append($"Telemetry: {await _linkShortenerService.GetShortcutLinkAsync(info.Telemetry.FlightClub)}\r\n");
            }

            if (info.Links.VideoLink != null)
            {
                linksDataBuilder.Append($"YouTube: {await _linkShortenerService.GetShortcutLinkAsync(info.Links.VideoLink)}\r\n");
            }

            return linksDataBuilder.ToString();
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
