﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using InElonWeTrust.Core.Helpers.Extensions;
using InElonWeTrust.Core.Services.Cache.Exceptions;
using NLog;

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
                    var data = await dataProvider(parameter);

                    _items.TryAdd(new Tuple<CacheContentType, string>(type, parameter), new CacheItem(data));
                    _cacheItemsAdded++;

                    return (TData)data;
                }

                var cachedItem = _items[new Tuple<CacheContentType, string>(type, parameter)];
                var lifetimeAttribute = type.GetEnumMemberAttribute<CacheLifetimeAttribute>(type);

                if ((DateTime.Now - cachedItem.UpdateTime).TotalMinutes >= lifetimeAttribute.Lifetime)
                {
                    var data = await dataProvider(parameter);
                    cachedItem.Update(data);

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

        private void CacheStatsTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _logger.Info($"Cache stats: {_cacheItemsAdded} added, {_cacheItemsUpdated} updated, {_cacheItemsHit} hit");
        }
    }
}
