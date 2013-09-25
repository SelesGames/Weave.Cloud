using Common.Azure;
using Common.Azure.SmartBlobClient;
using Common.Caching;
using FeedIconGrabber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class IconUrlCache : IBasicCache<string, Task<string>>
    {
        Dictionary<string, string> hfFeedIconCache;

        public Task<string> Get(string key)
        {
            if (hfFeedIconCache.ContainsKey(key))
            {
                return Task.FromResult(hfFeedIconCache[key]);
            }

            else return null;
        }

        async Task<string> GetDynamically(string key)
        {
            var domainResolver = new DomainResolver();
            var domainUrl = await domainResolver.GetDomainUrl(key);

            var iconGrabber = new DomainIconAcquirer(domainUrl);
            var iconUrl = await iconGrabber.GetIconUrl();

            return iconUrl;
        }
    }
}
