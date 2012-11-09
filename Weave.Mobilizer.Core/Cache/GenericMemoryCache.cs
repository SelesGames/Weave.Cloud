using System;
using System.Runtime.Caching;

namespace Weave.Mobilizer.Core.Cache
{
    public class GenericMemoryCache<T> : IExtendedCache<string, T>
    {
        MemoryCache cache;
        public TimeSpan CacheTTL { get; private set; }

        public GenericMemoryCache(string cacheName)
        {
            cache = new MemoryCache(cacheName);
        }

        public T GetOrAdd(string key, Func<string, T> valueFactory)
        {
            T val;

            if (cache.Contains(key))
                val = (T)cache[key];

            else
            {
                val = valueFactory(key);
                var wasAddSuccessful = cache.Add(key, val, new CacheItemPolicy { SlidingExpiration = CacheTTL });
                if (!wasAddSuccessful)
                    val = (T)cache[key];
            }

            return val;
        }

        public void SetCacheTTLInMinutes(int minutes)
        {
            CacheTTL = TimeSpan.FromMinutes(minutes);
        }
    }
}
