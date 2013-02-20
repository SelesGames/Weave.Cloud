//using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.LibraryClient;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedCache : IDisposable
    {
        Dictionary<string, CachedFeed> feeds = new Dictionary<string, CachedFeed>();
        CompositeDisposable disposables = new CompositeDisposable();

        string feedLibraryUrl;
        DbClient dbClient;
        int highFrequencyRefreshSplit;
        TimeSpan highFrequencyRefreshPeriod;




        #region Constructors

        public FeedCache(string feedLibraryUrl, DbClient dbClient)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) 
                throw new ArgumentException("feedLibraryUrl cannot be null: FeedCache ctor");

            this.feedLibraryUrl = feedLibraryUrl;
            this.dbClient = dbClient;
        }

        public FeedCache(
            string feedLibraryUrl,
            DbClient dbClient,
            int highFrequencyRefreshSplit, 
            TimeSpan highFrequencyRefreshPeriod)

            : this(feedLibraryUrl, dbClient)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl))
                throw new ArgumentException("feedLibraryUrl cannot be null: FeedCache ctor");

            // set some default values
            this.highFrequencyRefreshPeriod = highFrequencyRefreshPeriod;
            this.highFrequencyRefreshSplit = highFrequencyRefreshSplit;
        }

        #endregion




        public async Task InitializeAsync()
        {
            var feedClient = new FeedLibraryClient(feedLibraryUrl);
            var libraryFeeds = await feedClient.GetFeedsAsync();

            var highFrequencyFeeds = libraryFeeds
                .Distinct()
                .Select(o => new CachedFeed(o.FeedName, o.FeedUri))
                .ToList();

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);

            var mediators = highFrequencyFeeds.Select(o => new HFeedDbMediator(dbClient, o));

            foreach (var mediator in mediators)
            {
                await mediator.LoadLatestNews();
            }
        }

        public FeedResult ToFeedResult(FeedRequest request)
        {
            var feedUrl = request.Url;
            if (feeds.ContainsKey(feedUrl))
            {
                var feed = feeds[feedUrl];
                return feed.ToFeedResult(request);
            }
            else
                return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
        }

        public bool Contains(string feedUrl)
        {
            return feeds.ContainsKey(feedUrl);
        }

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }
}
