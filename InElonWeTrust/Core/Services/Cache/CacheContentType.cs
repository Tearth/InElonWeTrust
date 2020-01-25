namespace InElonWeTrust.Core.Services.Cache
{
    public enum CacheContentType
    {
        [CacheLifetime(5)] NextLaunch,
        [CacheLifetime(15)] LatestLaunch,
        [CacheLifetime(15)] AllLaunches,
        [CacheLifetime(15)] PastLaunches,
        [CacheLifetime(15)] UpcomingLaunches,
        [CacheLifetime(15)] FailedStarts,
        [CacheLifetime(15)] FailedLandings, 
        [CacheLifetime(15)] LaunchesWithOrbit,
        [CacheLifetime(60 * 24 * 7)] CompanyInfo,
        [CacheLifetime(60 * 24 * 7)] CompanyHistory,
        [CacheLifetime(60 * 24 * 7)] Launchpads,
        [CacheLifetime(60 * 24 * 7)] Rockets,
        [CacheLifetime(60 * 24 * 7)] Changelog,
        [CacheLifetime(60)] Roadster,
        [CacheLifetime(60)] Cores,
        [CacheLifetime(60)] CoreInfo            
    }
}
