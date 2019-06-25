using System.Collections.Generic;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Helpers
{
    public static class LaunchComparer
    {
        public static bool IsLaunchTheSame(LaunchInfo first, LaunchInfo second)
        {
            if (first.MissionName == second.MissionName && first.FlightNumber == second.FlightNumber)
            {
                return true;
            }

            if (first.LaunchSite.SiteName != second.LaunchSite.SiteName || first.Rocket.Id != second.Rocket.Id)
            {
                return false;
            }

            var firstLaunchFeatures = GetLaunchFeatures(first);
            var secondLaunchFeatures = GetLaunchFeatures(second);
            var similarity = CalculateSimilarity(firstLaunchFeatures, secondLaunchFeatures);

            return similarity.Similar >= similarity.Total / 2;
        }

        private static List<string> GetLaunchFeatures(LaunchInfo launch)
        {
            return new List<string>
            {
                launch.FlightNumber.ToString(),
                launch.MissionName,
                launch.StaticFireDateUtc.ToString(),
                launch.Rocket.FirstStage.Cores[0].CoreSerial,
                launch.Rocket.SecondStage.Payloads.Count.ToString(),
                launch.Rocket.SecondStage.Payloads[0].PayloadId,
                launch.Rocket.SecondStage.Payloads[0].Orbit.ToString(),
                launch.Details
            };
        }

        private static (int Similar, int Total) CalculateSimilarity(List<string> firstLaunchFeatures, List<string> secondLaunchFeatures)
        {
            var similar = 0;
            var total = firstLaunchFeatures.Count;

            for (var i = 0; i < total; i++)
            {
                if (firstLaunchFeatures[i] == secondLaunchFeatures[i])
                {
                    similar++;
                }
            }

            return (similar, total);
        }
    }
}
