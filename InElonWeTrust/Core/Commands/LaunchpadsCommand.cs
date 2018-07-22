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
using Oddity;
using Oddity.API.Models.Launchpad;

namespace InElonWeTrust.Core.Commands
{
    [Commands(GroupType.Miscellaneous)]
    public class LaunchpadsCommand
    {
        private readonly OddityCore _oddity;
        private readonly CacheService _cacheService;

        public LaunchpadsCommand(OddityCore oddity, CacheService cacheService)
        {
            _oddity = oddity;
            _cacheService = cacheService;

            _cacheService.RegisterDataProvider(CacheContentType.Launchpads, async p => await _oddity.Launchpads.GetAll().ExecuteAsync());
        }

        [Command("Launchpads")]
        [Description("Get list of all SpaceX launchpads.")]
        public async Task Launchpads(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var launchpads = await _cacheService.Get<List<LaunchpadInfo>>(CacheContentType.Launchpads);
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "List of SpaceX launchpads: ",
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var sortedLaunchpads = launchpads.OrderBy(p => p.FullName);
            var lastLaunchpad = sortedLaunchpads.Last();

            foreach (var launchpad in sortedLaunchpads)
            {
                var responseBuilder = new StringBuilder();
                var latitude = launchpad.Location.Latitude.Value.ToString(CultureInfo.InvariantCulture);
                var longitude = launchpad.Location.Longitude.Value.ToString(CultureInfo.InvariantCulture);

                responseBuilder.Append($"**[GOOGLE MAPS](https://maps.google.com/maps?q={latitude}+{longitude}&t=k)**. ");
                responseBuilder.Append(launchpad.Details);
                responseBuilder.Append("\r\n");

                if (launchpad != lastLaunchpad)
                {
                    responseBuilder.Append('\u200b');
                }

                var title = ":stadium: " + launchpad.FullName;
                switch (launchpad.Status)
                {
                    case LaunchpadStatus.UnderConstruction: title += " (under construction)";
                        break;

                    case LaunchpadStatus.Retired: title += " (retired)";
                        break;
                }

                embedBuilder.AddField(title, responseBuilder.ToString());
            }

            await ctx.RespondAsync("", false, embedBuilder);
        }
    }
}
