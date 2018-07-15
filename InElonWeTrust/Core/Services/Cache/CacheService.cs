using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using NLog;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheService<T>
    {
        private Dictionary<T, CacheItem> _items;
        private Timer _cacheStatsTimer;
        private int _cacheItemsAdded;
        private int _cacheItemsHitted;
        private int _cacheItemsUpdated;

        private Logger _logger = LogManager.GetCurrentClassLogger();

        private const int CacheItemLifeLengthMinutes = 30;
        private const int IntervalMinutes = 15;

        public CacheService()
        {
            _items = new Dictionary<T, CacheItem>();

            _cacheStatsTimer = new Timer(IntervalMinutes * 60 * 1000);
            _cacheStatsTimer.Elapsed += CacheStatsTimerOnElapsed;
            _cacheStatsTimer.Start();
        }

        public async Task<D> GetAndUpdateAsync<D>(T type, Func<Task<D>> dataProviderDelegate)
        {
            if (!_items.ContainsKey(type))
            {
                var data = await dataProviderDelegate();
                _items.Add(type, new CacheItem(data));

                _cacheItemsAdded++;
                return data;
            }

            var cachedItem = _items[type];
            if ((DateTime.Now - cachedItem.UpdateTime).TotalMinutes >= CacheItemLifeLengthMinutes)
            {
                var data = await dataProviderDelegate();
                cachedItem.Update(data);

                _cacheItemsUpdated++;
            }
            else
            {
                _cacheItemsHitted++;
            }

            return (D)cachedItem.Data;
        }

        private void CacheStatsTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _logger.Info($"Cache stats for {typeof(T).Name}: {_cacheItemsAdded} added, {_cacheItemsUpdated} updated, {_cacheItemsHitted} hitted");
        }
    }
}
