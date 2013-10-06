using Common.Azure.ServiceBus;
using Common.Azure.ServiceBus.Reactive;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedCache : IDisposable
    {
        Dictionary<string, CachedFeed> feeds = new Dictionary<string, CachedFeed>();
        List<HFeedDbMediator> mediators = new List<HFeedDbMediator>();

        CompositeDisposable disposables = new CompositeDisposable();
        object syncObject = new object();

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




        public CachedFeed Get(string feedUrl)
        {
            lock (syncObject)
            {
                if (feeds.ContainsKey(feedUrl))
                {
                    var feed = feeds[feedUrl];
                    if (feed.LastFeedState != CachedFeed.FeedState.Uninitialized)
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
                .Distinct()
                .ToList();

            var cachedFeeds = new List<CachedFeed>();

            foreach (var o in libraryFeeds)
            {
                var cachedFeed = new CachedFeed(o.FeedName, o.FeedUri);

                cachedFeeds.Add(cachedFeed);
                feeds.Add(o.FeedUri, cachedFeed);

                if (!string.IsNullOrWhiteSpace(o.CorrectedUri))
                {
                    feeds.Add(o.CorrectedUri, cachedFeed);
                }
            }

            mediators = cachedFeeds.Select(cachedFeed => new HFeedDbMediator(dbClient, cachedFeed)).ToList();

            foreach (var mediator in mediators)
            {
                await mediator.LoadLatestNews();
            }

            var client = await subscriptionConnector.CreateClient();

            client
                .AsObservable()
                .Select(Parse)
                .OfType<FeedUpdateNotice>()
                .Subscribe(OnFeedUpdateReceived, OnError);
        }

        void OnFeedUpdateReceived(FeedUpdateNotice notice)
        {
            foreach (var mediator in mediators)
            {
                mediator.ProcessFeedUpdateNotice(notice);
            }
        }

        void OnError(Exception exception)
        {
            DebugEx.WriteLine(exception);
        }




        #region private helper methods

        FeedUpdateNotice Parse(BrokeredMessage message)
        {
            FeedUpdateNotice notice = null;

            try
            {
                var properties = message.Properties;
                var id = message.MessageId;

                if (!EnumerableEx.IsNullOrEmpty(properties) &&
                    properties.ContainsKey("FeedId") &&
                    properties.ContainsKey("RefreshTime"))
                {
                    var feedId = (Guid)properties["FeedId"];
                    var refreshTime = (DateTime)properties["RefreshTime"];
                    var feedUri = (string)properties["FeedUri"];

                    notice = new FeedUpdateNotice(message)
                    {
                        MessageId = message.MessageId,
                        FeedId = feedId,
                        FeedUri = feedUri,
                        RefreshTime = refreshTime,
                    };
                }
            }
            catch { }

            return notice;
        }

        #endregion




        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}