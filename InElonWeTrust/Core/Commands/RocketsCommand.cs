using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Attributes;
using InElonWeTrust.Core.Commands.Definitions;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using InElonWeTrust.Core.Services.Pagination;
using Oddity;
using Oddity.API.Models.Launchpad;
using Oddity.API.Models.Rocket;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class RocketsCommand
    {
        private OddityCore _oddity;
        private CacheService _cacheService;

        private const int FieldLength = 30;

        public RocketsCommand(OddityCore oddity, CacheService cacheService)
        {
            _oddity = oddity;
            _cacheService = cacheService;

            _cacheService.RegisterDataProvider(CacheContentType.Rockets, async p => await _oddity.Rockets.GetAll().ExecuteAsync());
        }

        [Command("Rockets")]
        [Description("Get list of all SpaceX rockets.")]
        public async Task Rockets(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var rockets = await _cacheService.Get<List<RocketInfo>>(CacheContentType.Rockets);
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

            await ctx.RespondAsync("", false, embedBuilder);
        }

        private string GetTableWithRocketParameters(RocketInfo rocket)
        {
            var tableBuilder = new StringBuilder();

            tableBuilder.Append("```");
            tableBuilder.Append($"Diameter: {rocket.Diameter.Meters.Value}m".PadRight(FieldLength));
            tableBuilder.Append($"Launch cost: ${rocket.CostPerLaunch.Value / 1_000_000}kk\r\n");
            tableBuilder.Append($"First flight: {rocket.FirstFlight:MM-dd-yyyy}".PadRight(FieldLength));
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
