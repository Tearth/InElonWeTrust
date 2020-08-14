using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.Models.Cores;
using Oddity.Models.Launches;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class CoreInfoEmbedGenerator
    {
        public DiscordEmbed Build(CoreInfo core)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{core.Serial} (block {core.Block?.ToString() ?? "none"})",
                Description = $"{core.LastUpdate ?? "*No description at this moment :(*"}",
                Color = new DiscordColor(Constants.EmbedColor),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = Constants.SpaceXLogoImage
                }
            };

            var firstLaunch = core.Launches.Count > 0 ? core.Launches[0].Value : null;

            embed.AddField(":clock4: First launch time (UTC)", firstLaunch?.DateUtc?.ToString("D", CultureInfo.InvariantCulture) ?? "not launched yet", true);
            embed.AddField(":stadium: Current status", core.Status.ToString(), true);
            embed.AddField(":recycle: Landings", GetLandingsData(core));
            embed.AddField($":rocket: Missions ({core.Launches.Count})", GetMissionsList(core.Launches));
            embed.AddField("\u200b", "*Type `e!GetLaunch number` (e.g. e!GetLaunch 45) to get more detailed info about the mission.*");

            return embed;
        }

        public DiscordEmbed BuildError()
        {
            return new DiscordEmbedBuilder
            {
                Title = ":octagonal_sign: Oops",
                Description = "Core with the specified serial number doesn't exists! Type `e!Cores` to list them.",
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };
        }

        private string GetMissionsList(List<Lazy<LaunchInfo>> missions)
        {
            return missions.Any() ? string.Join("\r\n", missions.Select(p => $"{p.Value.FlightNumber}. {p.Value.Name}")) : "not launched yet";
        }

        private string GetLandingsData(CoreInfo core)
        {
            var landings = new List<string>();

            if (core.AsdsAttempts > 0)
            {
                landings.Add($"ASDS attempts: {core.AsdsAttempts} ({core.AsdsLandings} with success)");
            }

            if (core.RtlsAttempts > 0)
            {
                landings.Add($"RTLS attempts: {core.RtlsAttempts} ({core.RtlsLandings} with success)");
            }

            return landings.Any() ? string.Join("\r\n", landings) : "none";
        }
    }
}
