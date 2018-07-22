using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;
using Oddity.API.Models.Launch.Rocket.SecondStage;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class LaunchInfoEmbedGenerator
    {
        public DiscordEmbed Build(LaunchInfo launch, bool informAboutSubscription)
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
            if (linksData.Length > 0)
            {
                embed.AddField(":newspaper: Links:", linksData);
            }

            if (informAboutSubscription)
            {
                embed.AddField("\u200b", "*Click below reaction to subscribe this flight and be notified on DM 10 minutes before the launch.*");
            }

            return embed;
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

            if (info.Links.VideoLink != null)
            {
                links.Add($"__**[YT stream]({info.Links.VideoLink})**__");
            }

            if (info.Links.Presskit != null)
            {
                links.Add($"[Presskit]({info.Links.Presskit})");
            }

            if (info.Telemetry.FlightClub != null)
            {
                links.Add($"[Telemetry]({info.Telemetry.FlightClub})");
            }

            if (info.Links.RedditCampaign != null)
            {
                links.Add($"[Campaign]({info.Links.RedditCampaign})");
            }

            if (info.Links.RedditLaunch != null)
            {
                links.Add($"[Launch]({info.Links.RedditLaunch})");
            }

            if (info.Links.RedditMedia != null)
            {
                links.Add($"[Media]({info.Links.RedditMedia})");
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
