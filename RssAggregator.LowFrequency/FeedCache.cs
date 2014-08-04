using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;
using Weave.User.Service.Redis.PubSub;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedCache
    {
        const string CHANNEL = "feedUpdate";

        Dictionary<string, CachedFeed> feeds = new Dictionary<string, CachedFeed>();
        List<HFeedDbMediator> mediators = new List<HFeedDbMediator>();

        object syncObject = new object();

        string feedLibraryUrl;
        DbClient dbClient;
        ConnectionMultiplexer cm;




        #region Constructor

        public FeedCache(string feedLibraryUrl, DbClient dbClient, ConnectionMultiplexer cm)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) throw new ArgumentException("feedLibraryUrl cannot be null: FeedCache ctor");
            if (dbClient == null) throw new ArgumentNullException("dbClient cannot be null: FeedCache ctor");
            if (cm == null) throw new ArgumentNullException("cm cannot be null: FeedCache ctor");

            this.feedLibraryUrl = feedLibraryUrl;
            this.dbClient = dbClient;
            this.cm = cm;
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
                .Where(o => !string.IsNullOrWhiteSpace(o.FeedName) && !string.IsNullOrWhiteSpace(o.FeedUri))
                .Distinct()
                //.Take(3)  // for debugging only
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

            var sub = cm.GetSubscriber();
            var observable = await sub.AsObservable(CHANNEL);
            observable
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

        FeedUpdateNotice Parse(RedisPubSubTuple o)
        {
            try
            {
                if (!o.Message.HasValue)
                    return null;

                var bytes = (byte[])o.Message;
                using (var ms = new MemoryStream(bytes))
                using (var br = new BinaryReader(ms))
                {
                    var notice = new FeedUpdateNotice();

                    notice.FeedId = new Guid(br.ReadBytes(16));
                    notice.RefreshTime = DateTime.FromBinary(br.ReadInt64());
                    notice.FeedUri = br.ReadString();

                    return notice;
                }
            }
            catch { }

            return null;
        }

        #endregion
    }
}