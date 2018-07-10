using System;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheItem<D>
    {
        public DateTime UpdateTime { get; private set; }
        public D Data { get; private set; }

        public CacheItem(D data)
        {
            UpdateTime = DateTime.Now;
            Data = data;
        }

        public void Update(D data)
        {
            Data = data;
            UpdateTime = DateTime.Now;
        }
    }
}
