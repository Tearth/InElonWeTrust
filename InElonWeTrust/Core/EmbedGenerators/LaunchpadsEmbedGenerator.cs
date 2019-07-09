using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using Oddity.API.Models.Launchpad;

namespace InElonWeTrust.Core.EmbedGenerators
{
    public class LaunchpadsEmbedGenerator
    {
        public DiscordEmbed Build(List<LaunchpadInfo> launchpads)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "List of SpaceX launchpads",
                Color = new DiscordColor(Constants.EmbedColor)
            };

            var sortedLaunchpads = launchpads.OrderBy(p => p.FullName).ToList();
            var lastLaunchpad = sortedLaunchpads.Last();

            foreach (var launchpad in sortedLaunchpads)
            {
                var responseBuilder = new StringBuilder();
                var latitude = launchpad.Location.Latitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                var longitude = launchpad.Location.Longitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

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
                    case LaunchpadStatus.UnderConstruction:
                        title += " (under construction)";
                        break;

                    case LaunchpadStatus.Retired:
                        title += " (retired)";
                        break;
                }

                embedBuilder.AddField(title, responseBuilder.ToString());
            }

            return embedBuilder;
        }
    }
}
