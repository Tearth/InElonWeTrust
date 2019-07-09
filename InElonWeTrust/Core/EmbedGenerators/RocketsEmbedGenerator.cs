using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Rocket;
using Oddity.API.Models.Rocket.PayloadWeights;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class RocketsEmbedGenerator
    {
        private const int FieldLength = 30;

        public DiscordEmbed Build(List<RocketInfo> rockets)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "List of SpaceX rockets",
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var lastRocket = rockets.Last();
            foreach (var rocket in rockets)
            {
                var responseBuilder = new StringBuilder();
                responseBuilder.Append(rocket.Description);
                responseBuilder.Append("\r\n");
                responseBuilder.Append(GetTableWithRocketParameters(rocket));

                if (rocket != lastRocket)
                {
                    responseBuilder.Append('\u200b');
                }

                var title = rocket.Name;
                if (rocket.Active.HasValue && rocket.Active.Value)
                {
                    title += " (active)";
                }

                embedBuilder.AddField(title, responseBuilder.ToString());
            }

            return embedBuilder;
        }

        private string GetTableWithRocketParameters(RocketInfo rocket)
        {
            var tableBuilder = new StringBuilder();

            tableBuilder.Append("```");
            tableBuilder.Append($"Diameter: {rocket.Diameter.Meters ?? 0} m".PadRight(FieldLength));
            tableBuilder.Append($"Launch cost: ${(rocket.CostPerLaunch ?? 0) / 1_000_000}kk\r\n");
            tableBuilder.Append($"First flight: {rocket.FirstFlight:dd-MM-yyyy}".PadRight(FieldLength));
            tableBuilder.Append($"Success rate: {rocket.SuccessRate ?? 0} %\r\n");
            tableBuilder.Append($"Mass: {(int)((rocket.Mass.Kilograms ?? 0f) / 1000)}t".PadRight(FieldLength));
            tableBuilder.Append($"Height: {rocket.Height.Meters ?? 0} m\r\n");

            if (rocket.Engines.ThrustSeaLevel != null && rocket.Engines.ThrustToWeight.HasValue)
            {
                tableBuilder.Append($"Thrust: {(int)((rocket.Engines.ThrustSeaLevel.Kilonewtons ?? 0) * (rocket.Engines.Number ?? 1))} kn".PadRight(FieldLength));
                tableBuilder.Append($"TWR: {(int)rocket.Engines.ThrustToWeight.Value}\r\n");
            }

            tableBuilder.Append("\r\n");
            tableBuilder.Append(GetRocketPayload(rocket.PayloadWeights));
            tableBuilder.Append("```\r\n");

            return tableBuilder.ToString();
        }

        private string GetRocketPayload(List<PayloadWeightInfo> payloads)
        {
            var payloadsFormatted = new List<string>();
            payloads.ForEach(p => payloadsFormatted.Add($"{(int)((p.Kilograms ?? 0) / 1000)} tons to {p.Type}"));

            return string.Join("\r\n", payloadsFormatted);
        }
    }
}
