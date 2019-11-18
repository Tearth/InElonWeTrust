using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InElonWeTrust.Core.Helpers.Extensions;
using InElonWeTrust.Core.Helpers.Formatters;
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
                {CacheContentType.UpcomingLaunches, "List of all upcoming launches"},
                {CacheContentType.PastLaunches, "List of all past launches"},
                {CacheContentType.AllLaunches, "List of all launches"},
                {CacheContentType.FailedStarts, "List of all failed starts"},
                {CacheContentType.FailedLandings, "List of all failed landings"},
                {CacheContentType.LaunchesWithOrbit, "List of launches with the specified orbit"}
            };
        }

        public string Build(List<LaunchInfo> launches, CacheContentType contentType, int currentPage, string paginationFooter)
        {
            launches = launches.OrderBy(p => p.FlightNumber ?? 0).ToList();

            var launchesListBuilder = new StringBuilder();
            launchesListBuilder.Append($":rocket:  **{_listHeader[contentType]}**");
            launchesListBuilder.Append("\r\n");

            launchesListBuilder.Append("```");

            launchesListBuilder.Append("No. ".PadRight(MissionNumberLength));
            launchesListBuilder.Append("Mission name".PadRight(MissionNameLength));
            launchesListBuilder.Append("Launch time UTC".PadRight(LaunchDateLength));
            launchesListBuilder.Append("Launch site".PadRight(SiteNameLength));
            launchesListBuilder.Append("Landing".PadRight(LandingLength));
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(new string('-', TotalLength));
            launchesListBuilder.Append("\r\n");

            foreach (var launch in launches)
            {
                var launchDateTime = DateFormatter.GetDateStringWithPrecision(
                    launch.LaunchDateUtc ?? DateTime.MinValue,
                    launch.TentativeMaxPrecision ?? TentativeMaxPrecision.Year,
                    false, false, false);

                launchesListBuilder.Append($"{launch.FlightNumber ?? 0}.".PadRight(MissionNumberLength));
                launchesListBuilder.Append(launch.MissionName.ShortenString(MissionNameLength - 2).PadRight(MissionNameLength));
                launchesListBuilder.Append(launchDateTime.PadRight(LaunchDateLength));
                launchesListBuilder.Append(launch.LaunchSite.SiteName.PadRight(SiteNameLength));

                if (launch.TentativeMaxPrecision == TentativeMaxPrecision.Hour && launch.LaunchDateUtc < DateTime.UtcNow)
                {
                    var landing = launch.Rocket.FirstStage.Cores.Any(p => p.LandingType != null && p.LandingType != LandingType.Ocean);
                    launchesListBuilder.Append(landing.ConvertToYesNo(false).PadRight(LandingLength));
                    launchesListBuilder.Append("\r\n");
                }
                else
                {
                    launchesListBuilder.Append("?\r\n");
                }
            }

            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("Type \"e!GetLaunch number\" (e.g. e!GetLaunch 45) to get more information.");
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append(paginationFooter);
            launchesListBuilder.Append("\r\n");
            launchesListBuilder.Append("```");

            return launchesListBuilder.ToString();
        }
    }
}
