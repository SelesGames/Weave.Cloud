using Common.Azure.ServiceBus.Reactive;
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
        SubscriptionConnector subscriptionConnector;




        #region Constructor

        public FeedCache(string feedLibraryUrl, DbClient dbClient, SubscriptionConnector subscriptionConnector)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) throw new ArgumentException("feedLibraryUrl cannot be null: FeedCache ctor");
            if (dbClient == null) throw new ArgumentNullException("dbClient cannot be null: FeedCache ctor");
            if (subscriptionConnector == null) throw new ArgumentNullException("subscriptionConnector cannot be null: FeedCache ctor");

            this.feedLibraryUrl = feedLibraryUrl;
            this.dbClient = dbClient;
            this.subscriptionConnector = subscriptionConnector;
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
            var observable = await subscriptionConnector.CreateObservable();

            foreach (var mediator in mediators)
            {
                await mediator.LoadLatestNews();
                mediator.Subscribe(observable);
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

        public bool ContainsValid(string feedUrl)
        {
            if (feeds.ContainsKey(feedUrl))
            {
                var feed = feeds[feedUrl];
                if (feed.LastFeedState != CachedFeed.FeedState.Uninitialized)
                    return true;
            }

            return false;
        }

        public CachedFeed Get(string feedUrl)
        {
            return feeds[feedUrl];
        }

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }
}
