﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using InElonWeTrust.Core.Helpers;
using InElonWeTrust.Core.Services.Cache;
using Oddity.API.Models.Launch;
using Oddity.API.Models.Launch.Rocket.FirstStage;

namespace InElonWeTrust.Core.TableGenerators
{
    public class LaunchesListTableGenerator
    {
        private readonly Dictionary<CacheContentType, string> _listHeader;

        private const int MissionNumberLength = 4;
        private const int MissionNameLength = 23;
        private const int LaunchDateLength = 21;
        private const int SiteNameLength = 18;
        private const int LandingLength = 7;

        private const int TotalLength = MissionNumberLength + MissionNameLength + LaunchDateLength + SiteNameLength + LandingLength;

        public LaunchesListTableGenerator()
        {
            _listHeader = new Dictionary<CacheContentType, string>
            {
                {CacheContentType.UpcomingLaunches, "List of all upcoming launches:"},
                {CacheContentType.PastLaunches, "List of all past launches:"},
                {CacheContentType.AllLaunches, "List of all launches:"},
                {CacheContentType.FailedStarts, "List of all failed starts:"},
                {CacheContentType.FailedLandings, "List of all failed landings:"},
                {CacheContentType.LaunchesWithOrbit, "List of launches with the specified orbit:"}
            };
        }

        public string Build(List<LaunchInfo> launches, CacheContentType contentType, int currentPage, string paginationFooter)
        {
            var launchesListBuilder = new StringBuilder();
            launchesListBuilder.Append($":rocket:  **{_listHeader[contentType]}**");
            launchesListBuilder.Append("\r\n");

            launchesListBuilder.Append("```");

            launchesListBuilder.Append("No. ".PadRight(MissionNumberLength));
            launchesListBuilder.Append("Mission name".PadRight(MissionNameLength));
            launchesListBuilder.Append("Launch date UTC".PadRight(LaunchDateLength));
            launchesListBuilder.Append("Launch site".PadRight(SiteNameLength));
            launchesListBuilder.Append("Landing".PadRight(LandingLength));
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(new string('-', TotalLength));
            launchesListBuilder.Append("\r\n");

            foreach (var launch in launches)
            {
                launchesListBuilder.Append($"{launch.FlightNumber.Value}.".PadRight(MissionNumberLength));
                launchesListBuilder.Append(launch.MissionName.ShortenString(MissionNameLength - 5).PadRight(MissionNameLength));
                launchesListBuilder.Append(launch.LaunchDateUtc.Value.ToString("dd-MM-yy HH:mm:ss").PadRight(LaunchDateLength));
                launchesListBuilder.Append(launch.LaunchSite.SiteName.PadRight(SiteNameLength));

                var landing = launch.Rocket.FirstStage.Cores.Any(p => p.LandingType != null && p.LandingType != LandingType.Ocean);
                launchesListBuilder.Append((landing ? "yes" : "no").PadRight(LandingLength));
                launchesListBuilder.Append("\r\n");
            }

            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("Type e!getlaunch <number> to get more information.");
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(paginationFooter);
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("```");

            return launchesListBuilder.ToString();
        }
    }
}
