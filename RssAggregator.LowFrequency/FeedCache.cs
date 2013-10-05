using Common.Azure.ServiceBus;
using Common.Azure.ServiceBus.Reactive;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.LibraryClient;
using System.Reactive.Linq;

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

                    notice = new FeedUpdateNotice(message)
                    {
                        FeedId = feedId,
                        RefreshTime = refreshTime,
                    };
                }
            }
            catch { }

            return notice;
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

            var client = await subscriptionConnector.CreateClient();
            var observable = client.AsObservable();//.Select(Parse).OfType<FeedUpdateNotice>();

            foreach (var cachedFeed in cachedFeeds)
            {
                var mediator = new HFeedDbMediator(dbClient, cachedFeed);
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
