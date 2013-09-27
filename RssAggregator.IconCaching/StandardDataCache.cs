using Common.Caching;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using Weave.User.Service.Cache.Extensions;


namespace RssAggregator.IconCaching
{
    public class StandardDataCache<T> : IBasicCache<string, T>, IExtendedCache<string, T>
    {
        DataCache cache;

        public StandardDataCache(DataCache cache)
        {
            this.cache = cache;
        }

        public T Get(string key)
        {
            var o = SafeCacheGet(key);
            if (o != null)
            {
                return o.Cast<T>();
            }
            else
            {
                throw new KeyNotFoundException(string.Format("{0} not found in StandardDataCache", key));
            }
        }

        public T GetOrAdd(string key, Func<string, T> valueFactory)
        {
            var o = SafeCacheGet(key);
            if (o != null)
            {
                return o.Cast<T>();
            }

            // there was a cache miss if we get this far

            var x = valueFactory(key);

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
    }
}
