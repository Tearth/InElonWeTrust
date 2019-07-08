using System.Collections.Generic;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Helpers.Comparers
{
    public static class LaunchComparer
    {
        public static bool IsLaunchTheSame(LaunchInfo first, LaunchInfo second)
        {
            if (first.MissionName == second.MissionName && first.FlightNumber == second.FlightNumber)
            {
                return true;
            }

            if (first.LaunchSite.SiteId != second.LaunchSite.SiteId || first.Rocket.Id != second.Rocket.Id)
            {
                return false;
            }

            var firstLaunchFeatures = GetLaunchFeatures(first);
            var secondLaunchFeatures = GetLaunchFeatures(second);
            var similarity = CalculateSimilarity(firstLaunchFeatures, secondLaunchFeatures);

            return similarity.similar >= similarity.total / 2;
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
                launch.Links.VideoLink,
                launch.Links.Presskit,
                launch.Details
            };
        }

        private static (int similar, int total) CalculateSimilarity(List<string> firstLaunchFeatures, List<string> secondLaunchFeatures)
        {
            var similar = 0;
            var total = 0;

            for (var i = 0; i < firstLaunchFeatures.Count; i++)
            {
                if (firstLaunchFeatures[i] == null && secondLaunchFeatures[i] == null)
                {
                    continue;
                }

                if (firstLaunchFeatures[i] == secondLaunchFeatures[i])
                {
                    similar++;
                }

                total++;
            }

            return (similar, total);
        }
    }
}
