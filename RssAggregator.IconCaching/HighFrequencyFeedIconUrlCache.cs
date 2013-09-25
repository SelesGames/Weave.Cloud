using Common.Azure.SmartBlobClient;
using Common.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class HighFrequencyFeedIconUrlCache : IBasicCache<string, Task<string>>
    {
        bool isCacheLoaded = false;
        string feedUrl;
        Dictionary<string, string> feedIconLookup;
        SmartBlobClient blobClient;
        string blobName;

        public HighFrequencyFeedIconUrlCache(string feedUrl, SmartBlobClient blobClient, string blobName)
        {
            this.feedUrl = feedUrl;
            this.blobClient = blobClient;
            this.blobName = blobName;
        }

        async Task LoadFeedIcons()
        {
            var list = await blobClient.Get<FeedUrlIconMappings>(blobName);
            feedIconLookup = list.ToDictionary(o => o.Url, o => o.IconUrl);
            isCacheLoaded = true;
        }

        public async Task<string> Get(string key)
        {
            if (!isCacheLoaded)
            {
                await LoadFeedIcons();
            }

            return feedIconLookup[key];
        }
    }
}
