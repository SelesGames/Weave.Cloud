using Common.Caching;
using FeedIconGrabber;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class DynamicIconUrlCache : IBasicCache<string, Task<string>>, IExtendedCache<string, Task<string>>
    {
        string FALLBACK = "http://weave.blob.core.windows.net/icons/000.fallback.rss.jpg";

        public async Task<string> Get(string key)
        {
            string iconUrl = null;

            try
            {
                iconUrl = await GetDynamically(key);
            }
            catch { }

            if (string.IsNullOrWhiteSpace(iconUrl))
            {
                iconUrl = FALLBACK;
            }

            return iconUrl;
        }

        public Task<string> GetOrAdd(string key, System.Func<string, Task<string>> valueFactory)
        {
            return Get(key);
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
