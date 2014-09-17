using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;

namespace Weave.FeedUpdater.Service
{
    public class FeedCache
    {
        readonly string feedLibraryUrl;
        readonly Dictionary<string, CachedFeed> feeds = new Dictionary<string, CachedFeed>();
        readonly object syncObject = new object();




        #region Constructor

        public FeedCache(string feedLibraryUrl)//, ConnectionMultiplexer cm)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) throw new ArgumentException("feedLibraryUrl cannot be null: FeedCache ctor");
            this.feedLibraryUrl = feedLibraryUrl;
        }

        #endregion




        public CachedFeed Get(string feedUrl)
        {
            lock (syncObject)
            {
                if (feeds.ContainsKey(feedUrl))
                {
                    var feed = feeds[feedUrl];
                    return feed;
                }
            }

            return null;
        }

        public async Task InitializeAsync()
        {
            var feedClient = new FeedLibraryClient(feedLibraryUrl);

            await feedClient.LoadFeedsAsync();

            var libraryFeeds = feedClient.Feeds
                .Where(o => !string.IsNullOrWhiteSpace(o.FeedName) && !string.IsNullOrWhiteSpace(o.FeedUri))
                .Distinct()
                //.Take(3)  // for debugging only
                .ToList();

            foreach (var o in libraryFeeds)
            {
                var cachedFeed = new CachedFeed(o.FeedName, o.FeedUri);

                feeds.Add(o.FeedUri, cachedFeed);

                if (!string.IsNullOrWhiteSpace(o.CorrectedUri))
                {
                    feeds.Add(o.CorrectedUri, cachedFeed);
                }
            }
        }
    }
}