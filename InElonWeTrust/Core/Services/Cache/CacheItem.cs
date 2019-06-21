using System;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheItem
    {
        public DateTime UpdateTime { get; private set; }
        public object Data { get; private set; }

        public CacheItem(object data)
        {
            Update(data);
        }

        public void Update(object data)
        {
            Data = data;
            UpdateTime = DateTime.Now;
        }
    }
}
