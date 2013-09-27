using Common.Caching;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class IconUrlDataCache : IExtendedCache<string, Task<string>>
    {
        readonly string CACHE_NAME = "iconurls";

        StandardAsyncDataCache<string> dataCache;

        public IconUrlDataCache()
        {
            var config = new DataCacheFactoryConfiguration();
            var cacheFactory = new DataCacheFactory(config);
            var cache = cacheFactory.GetCache(CACHE_NAME);

            dataCache = new StandardAsyncDataCache<string>(cache);
        }

        public Task<string> GetOrAdd(string key, Func<string, Task<string>> valueFactory)
        {
            return dataCache.GetOrAdd(key, valueFactory);
        }
    }
}
