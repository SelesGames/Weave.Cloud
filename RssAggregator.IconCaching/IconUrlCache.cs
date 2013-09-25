using Common.Caching;
using FeedIconGrabber;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class IconUrlCache : IBasicCache<string, Task<string>>
    {
        string FALLBACK = "http://weave.blob.core.windows.net/icons/000.fallback.rss.jpg";

        Dictionary<string, string> hfFeedIconCache;

        public Task<string> Get(string key)
        {
            return Task.FromResult(FALLBACK);
            //if (hfFeedIconCache.ContainsKey(key))
            //{
            //    return Task.FromResult(hfFeedIconCache[key]);
            //}

            //else return null;
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
