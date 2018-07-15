using System;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheItem
    {
        public DateTime UpdateTime { get; private set; }
        public object Data { get; private set; }

        public CacheItem(object data)
        {
            UpdateTime = DateTime.Now;
            Data = data;
        }

        public void Update(object data)
        {
            Data = data;
            UpdateTime = DateTime.Now;
        }
    }
}
