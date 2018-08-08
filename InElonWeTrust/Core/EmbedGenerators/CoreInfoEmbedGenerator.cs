using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.DetailedCore;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class CoreInfoEmbedGenerator
    {
        public DiscordEmbed Build(DetailedCoreInfo core)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedColor),
                ThumbnailUrl = Constants.SpaceXLogoImage
            };

            embed.AddField($"{core.CoreSerial} (block {core.Block.ToString() ?? "none"})", $"{core.Details ?? "*No description at this moment :(*"}");
            embed.AddField(":clock4: First launch date (UTC):", core.OriginalLaunch?.ToUniversalTime().ToString("F", CultureInfo.InvariantCulture) ?? "not launched yet", true);
            embed.AddField(":stadium: Current status:", core.Status.ToString(), true);
            embed.AddField(":recycle: Landings:", GetLandingsData(core));
            embed.AddField($":rocket: Missions ({core.Missions.Count}):", GetMissionsList(core.Missions));

            return embed;
        }

        public DiscordEmbed BuildError()
        {
            var errorEmbedBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(Constants.EmbedErrorColor)
            };

            errorEmbedBuilder.AddField(":octagonal_sign: Oops", "Core with the specified serial number doesn't exists! Type `e!Cores` to list them.");
            return errorEmbedBuilder;
        }

        private string GetMissionsList(List<string> missions)
        {
            return missions.Any() ? string.Join(", ", missions) : "not launched yet";
        }

        private string GetLandingsData(DetailedCoreInfo core)
        {
            var landings = new List<string>();

            if (core.AsdsAttempt ?? false)
            {
                landings.Add("ASDS landings: " + core.AsdsLandings);
            }

            if (core.RtlsAttempt ?? false)
            {
                landings.Add("RTLS landings: " + core.RtlsLandings);
            }

            if (core.WaterLanding ?? false)
            {
                landings.Add("water landings: 1");
            }

            return landings.Any() ? string.Join(", ", landings) : "none";
        }
    }
}
