using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Helpers.Formatters;
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

                responseBuilder.Append($"**[[Map]({GoogleMapsLinkFormatter.GetGoogleMapsLink(launchpad.Location.Latitude ?? 0, launchpad.Location.Longitude ?? 0)})]** ");
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
