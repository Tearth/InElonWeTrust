using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Extensions;
using InElonWeTrust.Core.Helpers.Formatters;
using InElonWeTrust.Core.Services.TimeZone;
using Oddity.Models.Cores;
using Oddity.Models.Launches;
using Oddity.Models.Launchpads;
using Oddity.Models.Payloads;

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
                Title = $"{launch.FlightNumber}. {launch.Name} ({launch.Rocket.Value.Name} {launch.Rocket.Value.Type})",
                Description = launch.Details.ShortenString(1024) ?? "*No description at this moment :(*",
                Color = new DiscordColor(Constants.EmbedColor),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = launch.Links.Patch.Large ?? Constants.SpaceXLogoImage
                }
            };

            var launchDateTime = DateFormatter.GetDateStringWithPrecision(
                launch.DateUtc ?? DateTime.MinValue,
                launch.DatePrecision ?? DatePrecision.Year,
                true, true, true);

            embed.AddField(":clock4: Launch time (UTC)", launchDateTime, true);

            if (guildId != null)
            {
                var localLaunchDateTime = GetLocalLaunchDateTime(
                    guildId.Value,
                    launch.DateUtc ?? DateTime.MinValue,
                    launch.DatePrecision ?? DatePrecision.Year);

                var timeZoneName = _timeZoneService.GetTimeZoneForGuild(guildId.Value);

                if (timeZoneName != null)
                {
                    embed.AddField($":clock230: Launch time ({timeZoneName})", localLaunchDateTime);
                }
            }

            var googleMapsLink = $"[Map]({GoogleMapsLinkFormatter.GetGoogleMapsLink(launch.Launchpad.Value.Latitude ?? 0.0, launch.Launchpad.Value.Longitude ?? 0.0)})";
            embed.AddField(":stadium: Launchpad", $"{launch.Launchpad.Value.FullName} **[{googleMapsLink}]**");
            embed.AddField($":rocket: First stages ({1 + launch.Rocket.Value.Boosters})", GetCoresData(launch.Cores));
            embed.AddField($":package: Payloads ({launch.Payloads.Count})", GetPayloadsData(launch.Payloads));
            embed.AddField(":recycle: Reused parts", GetReusedPartsData(launch));

            var linksData = GetLinksData(launch);
            if (linksData.Length > 0)
            {
                embed.AddField(":newspaper: Links", linksData);
            }

            if (informAboutSubscription)
            {
                embed.AddField("\u200b", "*Click the reaction below to subscribe this flight and be notified on DM 10 minutes before the launch.*");
            }

            return embed;
        }

        private string GetLocalLaunchDateTime(ulong guildId, DateTime utc, DatePrecision precision)
        {
            var convertedToLocal = precision == DatePrecision.Hour
                ? _timeZoneService.ConvertUtcToLocalTime(guildId, utc)
                : utc;

            if (convertedToLocal == null)
            {
                return null;
            }

            return DateFormatter.GetDateStringWithPrecision(convertedToLocal.Value, precision, false, true, true);
        }

        private string GetPayloadsData(List<Lazy<PayloadInfo>> payloads)
        {
            var payloadsDataBuilder = new StringBuilder();
            foreach (var payload in payloads)
            {
                payloadsDataBuilder.Append(payload.Value.Name);

                if (payload.Value.MassKilograms != null)
                {
                    payloadsDataBuilder.Append($" ({payload.Value.MassKilograms} kg)");
                }

                if (payload.Value.Orbit != null)
                {
                    payloadsDataBuilder.Append($" to {payload.Value.Orbit}");
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

        private string GetCoresData(List<LaunchCoreInfo> launchCores)
        {
            var coresDataBuilder = new StringBuilder();
            var numerals = new[] {"first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "tenth"};

            foreach (var launchCore in launchCores)
            {
                var coreDefinition = launchCore.Core.Value;

                coresDataBuilder.Append($"{launchCore.Core.Value.Serial ?? "Unknown"}");
                if (launchCore.Core.Value.Block != null)
                {
                    coresDataBuilder.Append($" (block {coreDefinition.Block})");
                }

                if (coreDefinition.Launches != null && coreDefinition.Launches.Count > 0)
                {
                    coresDataBuilder.Append($", {numerals[coreDefinition.Launches.Count - 1]} flight");
                }

                if (launchCore.LandingType != null && launchCore.Landpad != null && launchCore.LandingType != "Ocean")
                {
                    coresDataBuilder.Append($", landing on {launchCore.Landpad.Value.Name}");
                }

                if (launchCore.LandingSuccess != null)
                {
                    coresDataBuilder.Append($" ({(launchCore.LandingSuccess.Value ? "success" : "fail")})");
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

            if (info.Links.Webcast != null)
            {
                links.Add($"__**[YT stream]({info.Links.Webcast})**__");
            }

            if (info.Links.Presskit != null)
            {
                links.Add($"[Presskit]({info.Links.Presskit})");
            }

            if (info.Links.Reddit.Campaign != null)
            {
                links.Add($"[Campaign]({info.Links.Reddit.Campaign})");
            }

            if (info.Links.Reddit.Launch != null)
            {
                links.Add($"[Launch]({info.Links.Reddit.Launch})");
            }

            if (info.Links.Reddit.Media != null)
            {
                links.Add($"[Media]({info.Links.Reddit.Media})");
            }

            return string.Join(", ", links);
        }

        private string GetReusedPartsData(LaunchInfo launch)
        {
            var reusedPartsList = new List<string>();

            if (launch.Cores[0].Reused ?? false)
            {
                reusedPartsList.Add("Core");
            }

            if (launch.Capsules.Count > 0 && launch.Capsules[0].Value.ReuseCount > 0)
            {
                reusedPartsList.Add("Capsule");
            }

            if (launch.Fairings.Reused ?? false)
            {
                reusedPartsList.Add("Fairings");
            }

            if (launch.Cores.Count == 3 && (launch.Cores[1].Reused ?? false))
            {
                reusedPartsList.Add("First side core");
            }

            if (launch.Cores.Count == 3 && (launch.Cores[2].Reused ?? false))
            {
                reusedPartsList.Add("Second side core");
            }

            return reusedPartsList.Count > 0 ? string.Join(", ", reusedPartsList) : "none";
        }
    }
}
