using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;

namespace Weave.RssAggregator
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
        }
    }
}




#region now deprecated code relating to processing feed updates from redis pubsub

//const string CHANNEL = "feedUpdate";
//List<HFeedDbMediator> mediators = new List<HFeedDbMediator>();
//ConnectionMultiplexer cm;
//            if (cm == null) throw new ArgumentNullException("cm cannot be null: FeedCache ctor");
//this.cm = cm;

                //mediators = cachedFeeds.Select(cachedFeed => new HFeedDbMediator(cachedFeed)).ToList();

            //foreach (var mediator in mediators)
            //{
            //    await mediator.LoadLatestNews();
            //}

            //var sub = cm.GetSubscriber();
            //var observable = await sub.AsObservable(CHANNEL);
            //observable
            //    .Select(Parse)
            //    .OfType<FeedUpdateNotice>()
            //    .Subscribe(OnFeedUpdateReceived, OnError);
        //}

        //void OnFeedUpdateReceived(FeedUpdateNotice notice)
        //{
        //    foreach (var mediator in mediators)
        //    {
        //        mediator.ProcessFeedUpdateNotice(notice);
        //    }
        //}

        //void OnError(Exception exception)
        //{
        //    DebugEx.WriteLine(exception);
        //}




        //#region private helper methods

        //FeedUpdateNotice Parse(RedisPubSubTuple o)
        //{
        //    try
        //    {
        //        if (!o.Message.HasValue)
        //            return null;

        //        var bytes = (byte[])o.Message;
        //        using (var ms = new MemoryStream(bytes))
        //        using (var br = new BinaryReader(ms))
        //        {
        //            var notice = new FeedUpdateNotice();

        //            notice.FeedId = new Guid(br.ReadBytes(16));
        //            notice.RefreshTime = DateTime.FromBinary(br.ReadInt64());
        //            notice.FeedUri = br.ReadString();

        //            return notice;
        //        }
        //    }
        //    catch { }

        //    return null;
        //}

        //#endregion

#endregion