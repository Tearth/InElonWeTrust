using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheService<T, D>
    {
        private Dictionary<T, CacheItem<D>> _items;
        private const int CacheItemLifeLength = 60;

        public CacheService()
        {
            _items = new Dictionary<T, CacheItem<D>>();
        }

        public async Task<D> GetAndUpdate(T type, Func<Task<D>> dataProviderDelegate)
        {
            if (!_items.ContainsKey(type))
            {
                var data = await dataProviderDelegate();
                _items.Add(type, new CacheItem<D>(data));

                return data;
            }

            var cachedItem = _items[type];
            if ((DateTime.Now - cachedItem.UpdateTime).TotalSeconds >= CacheItemLifeLength)
            {
                var data = await dataProviderDelegate();
                cachedItem.Update(data);
            }

            return cachedItem.Data;
        }
    }
}
