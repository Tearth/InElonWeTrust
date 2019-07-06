using System;

namespace InElonWeTrust.Core.Services.Cache
{
    public class CacheLifetimeAttribute : Attribute
    {
        public int Lifetime { get; }

        public CacheLifetimeAttribute(int lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
