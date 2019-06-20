using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.TimeZone;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;
using Oddity.API.Models.Launch.Rocket.SecondStage;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class LaunchInfoEmbedGenerator
    {
        private readonly TimeZoneService _timeZoneService;

        public LaunchInfoEmbedGenerator(TimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        public DiscordEmbed Build(LaunchInfo launch, ulong? guildId, bool informAboutSubscription)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = launch.Links.MissionPatch ?? Constants.SpaceXLogoImage
            };

            var launchDateTime = DateFormatter.GetStringWithPrecision(launch.LaunchDateUtc.Value, launch.TentativeMaxPrecision.Value, true, true);

            embed.AddField($"{launch.FlightNumber}. {launch.MissionName} ({launch.Rocket.RocketName} {launch.Rocket.RocketType})", launch.Details.ShortenString(1000) ?? "*No description at this moment :(*");
            embed.AddField(":clock4: Launch date (UTC)", launchDateTime, true);

            if (guildId != null)
            {
                var localLaunchDateTime = GetLocalLaunchDateTime(guildId.Value, launch.LaunchDateUtc.Value, launch.TentativeMaxPrecision.Value);
                var timeZoneName = _timeZoneService.GetTimeZoneForGuild(guildId.Value);

                if (localLaunchDateTime != null)
                {
                    embed.AddField($":clock230: Launch date ({timeZoneName})", localLaunchDateTime);
                }
            }

            embed.AddField(":stadium: Launchpad", launch.LaunchSite.SiteName);
            embed.AddField($":rocket: First stages ({launch.Rocket.FirstStage.Cores.Count})", GetCoresData(launch.Rocket.FirstStage.Cores));
            embed.AddField($":package: Payloads ({launch.Rocket.SecondStage.Payloads.Count})", GetPayloadsData(launch.Rocket.SecondStage.Payloads));
            embed.AddField(":recycle: Reused parts", GetReusedPartsData(launch.Reuse));

            var linksData = GetLinksData(launch);
            if (linksData.Length > 0)
            {
                embed.AddField(":newspaper: Links", linksData);
            }

            if (informAboutSubscription)
            {
                embed.AddField("\u200b", "*Click below reaction to subscribe this flight and be notified on DM 10 minutes before the launch.*");
            }

            return embed;
        }

        private string GetLocalLaunchDateTime(ulong guildId, DateTime utc, TentativeMaxPrecision precision)
        {
            var convertedToLocal = _timeZoneService.ConvertUtcToLocalTime(guildId, utc);
            if (convertedToLocal == null)
            {
                return null;
            }

            return DateFormatter.GetStringWithPrecision(convertedToLocal.Value, precision, false, true);
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
            var numerals = new[] {"first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "tenth"};

            foreach (var core in cores)
            {
                coresDataBuilder.Append($"{core.CoreSerial ?? "Unknown"}");
                if (core.Block != null)
                {
                    coresDataBuilder.Append($" (block {core.Block})");
                }

                if (core.Flight != null && core.Flight > 0)
                {
                    coresDataBuilder.Append($", {numerals[core.Flight.Value - 1]} flight");
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

            if (reused.Core ?? false)
            {
                reusedPartsList.Add("Core");
            }

            if (reused.Capsule ?? false)
            {
                reusedPartsList.Add("Capsule");
            }

            if (reused.Fairings ?? false)
            {
                reusedPartsList.Add("Fairings");
            }

            if (reused.FirstSideCore ?? false)
            {
                reusedPartsList.Add("First side core");
            }

            if (reused.SecondSideCore ?? false)
            {
                reusedPartsList.Add("Second side core");
            }

            return reusedPartsList.Count > 0 ? string.Join(", ", reusedPartsList) : "none";
        }
    }
}
