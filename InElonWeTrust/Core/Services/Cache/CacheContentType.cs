namespace InElonWeTrust.Core.Services.Cache
{
    public enum CacheContentType
    {
        [CacheLifetime(15)] NextLaunch,
        [CacheLifetime(15)] LatestLaunch,
        [CacheLifetime(15)] AllLaunches,
        [CacheLifetime(15)] PastLaunches,
        [CacheLifetime(15)] UpcomingLaunches,
        [CacheLifetime(15)] FailedStarts,
        [CacheLifetime(15)] FailedLandings, 
        [CacheLifetime(15)] LaunchesWithOrbit,
        [CacheLifetime(60 * 24)] CompanyInfo,
        [CacheLifetime(60 * 24)] CompanyHistory,
        [CacheLifetime(60 * 24)] Launchpads,
        [CacheLifetime(60)] Rockets,
        [CacheLifetime(60 * 24)] Changelog,
        [CacheLifetime(60)] Roadster,
        [CacheLifetime(60)] Cores,
        [CacheLifetime(60)] CoreInfo            
    }
}
