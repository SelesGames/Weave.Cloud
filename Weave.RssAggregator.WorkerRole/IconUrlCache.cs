using Common.Caching;
using FeedIconGrabber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.RssAggregator.WorkerRole
{
    public class IconUrlCache : IBasicCache<string, Task<string>>
    {
        public Task<string> Get(string key)
        {
            throw new NotImplementedException();
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
