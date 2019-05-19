using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Rocket;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class RocketsEmbedGenerator
    {
        private const int FieldLength = 30;

        public DiscordEmbedBuilder Build(List<RocketInfo> rockets)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "List of SpaceX rockets: ",
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
                if (rocket.Active.Value)
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
            tableBuilder.Append($"Diameter: {rocket.Diameter.Meters.Value}m".PadRight(FieldLength));
            tableBuilder.Append($"Launch cost: ${rocket.CostPerLaunch.Value / 1_000_000}kk\r\n");
            tableBuilder.Append($"First flight: {rocket.FirstFlight:dd-MM-yyyy}".PadRight(FieldLength));
            tableBuilder.Append($"Success rate: {rocket.SuccessRate.Value}%\r\n");
            tableBuilder.Append($"Mass: {(int)(rocket.Mass.Kilograms.Value / 1000)}t".PadRight(FieldLength));
            tableBuilder.Append($"Height: {rocket.Height.Meters.Value}m\r\n");

            if (rocket.Engines.ThrustSeaLevel != null && rocket.Engines.ThrustToWeight.HasValue)
            {
                tableBuilder.Append($"Thrust: {(int)(rocket.Engines.ThrustSeaLevel.Kilonewtons.Value * rocket.Engines.Number.Value)}kn".PadRight(FieldLength));
                tableBuilder.Append($"TWR: {(int)rocket.Engines.ThrustToWeight.Value}\r\n");
            }

            var lastPayload = rocket.PayloadWeights.Last();
            foreach (var payload in rocket.PayloadWeights)
            {
                tableBuilder.Append($"{(int)(payload.Kilograms / 1000)}t to {payload.Type}");

                if (payload != lastPayload)
                {
                    tableBuilder.Append(", ");
                }
            }

            tableBuilder.Append("```\r\n");

            return tableBuilder.ToString();
        }
    }
}
