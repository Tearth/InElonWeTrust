using System;
using System.Collections.Generic;
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
    public class NextLaunchCommand
    {
        private OddityCore _oddity;
        private LinkShortenerService _linkShortenerService;

        public NextLaunchCommand()
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

            var nextLaunchData = await _oddity.Launches.GetLatest().ExecuteAsync();
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = nextLaunchData.Links.MissionPatch ?? Constants.SpaceXLogoImage
            };

            embed.AddField($"{nextLaunchData.MissionName} ({nextLaunchData.Rocket.RocketName} {nextLaunchData.Rocket.RocketType})", nextLaunchData.Details);
            embed.AddField("Launch date:", nextLaunchData.LaunchDateUtc.Value.ToLongDateString(), true);
            embed.AddField($"Payloads ({nextLaunchData.Rocket.SecondStage.Payloads.Count}):", GetPayloadsData(nextLaunchData.Rocket.SecondStage.Payloads), true);
            embed.AddField("Launchpad:", nextLaunchData.LaunchSite.SiteName, true);
            embed.AddField($"First stages ({nextLaunchData.Rocket.FirstStage.Cores.Count}):", GetCoresData(nextLaunchData.Rocket.FirstStage.Cores), true);
            embed.AddField("Links:", await GetLinksData(nextLaunchData.Links));

            await ctx.RespondAsync("", false, embed);
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
                coresDataBuilder.Append($"{core.CoreSerial} (block {core.Block})");
                if (core.LandingType != null && core.LandingType != LandingType.Ocean)
                {
                    coresDataBuilder.Append($", landing on {core.LandingVehicle}");
                }

                coresDataBuilder.Append("\r\n");
            }

            return coresDataBuilder.ToString();
        }

        private async Task<string> GetLinksData(LinksInfo links)
        {
            var linksDataBuilder = new StringBuilder();
            linksDataBuilder.Append($"Reddit: {await _linkShortenerService.GetShortcutLinkAsync(links.RedditLaunch)}\r\n");
            linksDataBuilder.Append($"Presskit: {await _linkShortenerService.GetShortcutLinkAsync(links.Presskit)}\r\n");
            linksDataBuilder.Append($"YouTube: {await _linkShortenerService.GetShortcutLinkAsync(links.VideoLink)}\r\n");

            return linksDataBuilder.ToString();
        }
    }
}
