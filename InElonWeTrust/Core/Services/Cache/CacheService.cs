using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Helpers.Extensions;
using InElonWeTrust.Core.Services.Cache.Exceptions;
using NLog;
using Oddity.API.Models.Launch;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheService
    {
        private readonly ConcurrentDictionary<CacheContentType, Func<string, Task<object>>> _dataProviders;
        private readonly ConcurrentDictionary<Tuple<CacheContentType, string>, CacheItem> _items;

        private readonly Timer _cacheStatsTimer;
        private int _cacheItemsAdded;
        private int _cacheItemsHit;
        private int _cacheItemsUpdated;
        private TimeSpan? _customLaunchTime;
        private readonly System.Threading.SemaphoreSlim _cacheLock;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int CacheItemLifeLengthMinutes = 10;
        private const int CacheStatsIntervalMinutes = 15;

        public CacheService()
        {
            _dataProviders = new ConcurrentDictionary<CacheContentType, Func<string, Task<object>>>();
            _items = new ConcurrentDictionary<Tuple<CacheContentType, string>, CacheItem>();

            _cacheStatsTimer = new Timer(CacheStatsIntervalMinutes * 60 * 1000);
            _cacheStatsTimer.Elapsed += CacheStatsTimerOnElapsed;
            _cacheStatsTimer.Start();

            _cacheLock = new System.Threading.SemaphoreSlim(1, 1);
        }

        public void RegisterDataProvider(CacheContentType type, Func<string, Task<object>> dataProviderDelegate)
        {
            _dataProviders.TryAdd(type, dataProviderDelegate);
        }

        public async Task<TData> GetAsync<TData>(CacheContentType type, string parameter = null)
        {
            await _cacheLock.WaitAsync();

            try
            {
                if (!_dataProviders.TryGetValue(type, out var dataProvider))
                {
                    throw new NoDataProviderException($"{type} data provider doesn't exist in cache.");
                }

                if (!_items.ContainsKey(new Tuple<CacheContentType, string>(type, parameter)))
                {
                    var data = await FetchDataFromProvider(dataProvider, parameter, null);

                    _items.TryAdd(new Tuple<CacheContentType, string>(type, parameter), new CacheItem(data));
                    _cacheItemsAdded++;

                    _logger.Info($"Cache data added ({type})");

                    ApplyPatches(type, data);
                    return (TData)data;
                }

                var cachedItem = _items[new Tuple<CacheContentType, string>(type, parameter)];
                var lifetimeAttribute = type.GetEnumMemberAttribute<CacheLifetimeAttribute>(type);

                if ((DateTime.Now - cachedItem.UpdateTime).TotalMinutes >= lifetimeAttribute.Lifetime)
                {
                    var data = await FetchDataFromProvider(dataProvider, parameter, cachedItem.Data);
                    if (data != cachedItem.Data)
                    {
                        ApplyPatches(type, data);
                        cachedItem.Update(data);

                        _logger.Info($"Cache data updated ({type})");
                    }

                    _cacheItemsUpdated++;
                }
                else
                {
                    _cacheItemsHit++;
                }

                return (TData)cachedItem.Data;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public void SetCustomLaunchTime(TimeSpan time)
        {
            _customLaunchTime = time;
        }

        public void ResetCustomLaunchTime()
        {
            _customLaunchTime = null;
        }

        public TimeSpan? GetCustomLaunchTime()
        {
            return _customLaunchTime;
        }

        private async Task<object> FetchDataFromProvider(Func<string, Task<object>> provider, string parameter, object oldData)
        {
            Exception lastException;

            try
            {
                return await provider(parameter);
            }
            catch (Exception e)
            {
                _logger.Warn($"Failed to retrieve data from the provider ({e.GetType().Name}: {e.Message})");
                lastException = e;
            }

            if (oldData != null)
            {
                _logger.Warn("Returned old data from cache");
                return oldData;
            }

            if (lastException != null)
            {
                throw lastException;
            }

            return null;
        }

        private void ApplyPatches<TData>(CacheContentType type, TData data)
        {
            var patchesApplied = 0;
            switch (data)
            {
                case LaunchInfo launch when launch.FlightNumber == 87 && launch.Links.VideoLink == "https://youtu.be/pIDuv0Ta0XQ":
                {
                    launch.Links.VideoLink = null;
                    patchesApplied++;
                    break;
                }

                case LaunchInfo launch when type == CacheContentType.NextLaunch && _customLaunchTime.HasValue:
                {
                    launch.LaunchDateUtc = new DateTime(
                        launch.LaunchDateUtc.Value.Year,
                        launch.LaunchDateUtc.Value.Month,
                        launch.LaunchDateUtc.Value.Day,
                        _customLaunchTime.Value.Hours,
                        _customLaunchTime.Value.Minutes,
                        _customLaunchTime.Value.Seconds
                    );
                    patchesApplied++;
                    break;
                }

                /*
                case LaunchInfo launch when type == CacheContentType.NextLaunch && !_customLaunchTime.HasValue:
                {
                    launch.TentativeMaxPrecision = TentativeMaxPrecision.Month;
                    launch.LaunchDateUtc = new DateTime(2020, 01, 1, 0, 0, 0, DateTimeKind.Utc);
                    patchesApplied++;
                    break;
                }
                */
            }

            _logger.Info($"{patchesApplied} patches applied");
        }

        private void CacheStatsTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _logger.Info($"Cache stats: {_cacheItemsAdded} added, {_cacheItemsUpdated} updated, {_cacheItemsHit} hit");
        }
    }
}
