using Common.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class StandardIconCache : NLevelIconUrlCache
    {
        public StandardIconCache()
            : base(CreateCaches())
        { }

        static IExtendedCache<string, Task<string>>[] CreateCaches()
        {
            var hfCache = new HighFrequencyFeedIconUrlCache();
            hfCache.BeginListeningToResourceChanges();

            var caches = new IExtendedCache<string, Task<string>>[] 
            {
                hfCache,
                new IconUrlAzureDataCache(),
                new DynamicIconUrlCache()
            };
            return caches;
        }
    }
}