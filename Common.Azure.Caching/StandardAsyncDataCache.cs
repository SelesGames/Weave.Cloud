using Common.Azure.Caching.Extensions;
using Common.Caching;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Azure.Caching
{
    public class StandardAsyncDataCache<T> : IBasicCache<string, Task<T>>, IExtendedCache<string, Task<T>>
    {
        DataCache cache;

        public StandardAsyncDataCache(DataCache cache)
        {
            this.cache = cache;
        }

        public Task<T> Get(string key)
        {
            var o = SafeCacheGet(key);
            if (o != null)
            {
                return Task.FromResult(o.Cast<T>());
            }
            else
            {
                throw new KeyNotFoundException(string.Format("{0} not found in StandardDataCache", key));
            }
        }

        public async Task<T> GetOrAdd(string key, Func<string, Task<T>> valueFactory)
        {
            var o = SafeCacheGet(key);
            if (o != null)
            {
                return o.Cast<T>();
            }

            // there was a cache miss if we get this far

            var x = await valueFactory(key);

            cache.Put(key, x);

            return x;
        }

        object SafeCacheGet(string key)
        {
            object result = null;

            try
            {
                result = cache.Get(key);
            }
            catch { }

            return result;
        }

        object SafeCacheget(IEnumerable<string> keys)
        {
            object result = null;

            try
            {
                result = cache.BulkGet()
            }
        }
    }
}
