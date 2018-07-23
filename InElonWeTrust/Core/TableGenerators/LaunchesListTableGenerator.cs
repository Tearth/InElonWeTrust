using System;
using System.Collections.Generic;
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

        private const int _missionNumberLength = 4;
        private const int _missionNameLength = 23;
        private const int _launchDateLength = 21;
        private const int _siteNameLength = 18;
        private const int _landingLength = 7;

        private int _totalLength => _missionNumberLength + _missionNameLength + _launchDateLength + _siteNameLength + _landingLength;

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

            launchesListBuilder.Append("No. ".PadRight(_missionNumberLength));
            launchesListBuilder.Append("Mission name".PadRight(_missionNameLength));
            launchesListBuilder.Append("Launch date UTC".PadRight(_launchDateLength));
            launchesListBuilder.Append("Launch site".PadRight(_siteNameLength));
            launchesListBuilder.Append("Landing".PadRight(_landingLength));
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(new string('-', _totalLength));
            launchesListBuilder.Append("\r\n");

            foreach (var launch in launches)
            {
                launchesListBuilder.Append($"{launch.FlightNumber.Value}.".PadRight(_missionNumberLength));
                launchesListBuilder.Append(launch.MissionName.ShortenString(_missionNameLength - 5).PadRight(_missionNameLength));
                launchesListBuilder.Append(launch.LaunchDateUtc.Value.ToString("dd-MM-yy HH:mm:ss").PadRight(_launchDateLength));
                launchesListBuilder.Append(launch.LaunchSite.SiteName.PadRight(_siteNameLength));

                var landing = launch.Rocket.FirstStage.Cores.Any(p => p.LandingType != null && p.LandingType != LandingType.Ocean);
                launchesListBuilder.Append((landing ? "yes" : "no").PadRight(_landingLength));
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
