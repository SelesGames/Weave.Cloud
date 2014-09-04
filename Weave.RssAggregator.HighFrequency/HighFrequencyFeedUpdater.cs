using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedUpdater : IDisposable
    {
        Dictionary<string, HighFrequencyFeed> feeds = new Dictionary<string, HighFrequencyFeed>();

        readonly string feedLibraryUrl;
        readonly ConnectionMultiplexer connection;
        int currentRefreshIndex = 0;
        IDisposable subHandle;

        public TimeSpan RefreshPeriod { get; set; }

        readonly Queue<HighFrequencyFeed> queue = new Queue<HighFrequencyFeed>();
        readonly List<HighFrequencyFeed> processList = new List<HighFrequencyFeed>();
        readonly object sync = new object();



        #region Constructor

        public HighFrequencyFeedUpdater(
            string feedLibraryUrl, 
            ConnectionMultiplexer connection)
        {
            this.feedLibraryUrl = feedLibraryUrl;
            this.connection = connection;

            // set some default values
            RefreshPeriod = TimeSpan.FromMinutes(20);
        }

        #endregion


        void ProcessNext()
        {
            HighFrequencyFeed next;

            lock(sync)
            {
                next = queue.Dequeue()
            }
            var next = 
        }




        public async Task InitializeAsync()
        {
            var feedClient = new FeedLibraryClient(feedLibraryUrl);

            await feedClient.LoadFeedsAsync();
            var libraryFeeds = feedClient.Feeds;

            var highFrequencyFeeds = libraryFeeds
                .Distinct()
                //.Where(o => !string.IsNullOrWhiteSpace(o.CorrectedUri))
                .Where(o => o.FeedUri == "http://www.gameinformer.com:80/feeds/thefeedrss.aspx")
                .Select(CreateHighFrequencyFeed)
                .OfType<HighFrequencyFeed>()
                .ToList();

            var feedUrls = highFrequencyFeeds.Select(o => o.Uri);

            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);
            var cacheMultiGet = await cache.Get(feedUrls);

            //var ordered = cacheMultiGet.Results.OrderByDescending(o => o.ByteLength).ToList();
            //DebugEx.WriteLine(ordered);

            var recoveredFeedUpdaters = cacheMultiGet.GetValidValues().ToList();

            var joined = highFrequencyFeeds.Join(
                recoveredFeedUpdaters, 
                o => o.Uri.ToLowerInvariant(), 
                o => o.Uri.ToLowerInvariant(),
                (hff, updater) => new { hff, updater });

            foreach (var tuple in joined)
            {
                tuple.hff.InitializeWith(tuple.updater);
            }

            CreateLookup(highFrequencyFeeds);
        }

        HighFrequencyFeed CreateHighFrequencyFeed(FeedSource o)
        {
            try
            {
                string originalUri = null, feedUri = null;

                if (!string.IsNullOrWhiteSpace(o.CorrectedUri))
                {
                    originalUri = o.FeedUri;
                    feedUri = o.CorrectedUri;
                }
                else
                {
                    feedUri = o.FeedUri;
                }

                var hff = new HighFrequencyFeed(o.FeedName, feedUri, o.Instructions, originalUri == null ? null : new[] { originalUri });
                return hff;
            }
            catch
            {
                return null;
            }
        }

        void CreateLookup(IEnumerable<HighFrequencyFeed> highFrequencyFeeds)
        {
            foreach (var hff in highFrequencyFeeds)
            {
                var uris = hff.PreviousUris.Union(new[] { hff.Uri });
                foreach (var uri in uris)
                {
                    if (!feeds.ContainsKey(uri))
                        feeds.Add(uri, hff);
                }
            }
        }



#if DEBUG
        public Task RefreshAllFeedsImmediately()
        {
            var feedsList = feeds.Select(o => o.Value).ToList();
            return Task.WhenAll(feedsList.Select(o => o.Refresh(CreateProcessor())));
        }
#endif



        public async void StartFeedRefreshTimer()
        {
            if (subHandle != null)
                subHandle.Dispose();

            var feedsList = feeds.Select(o => o.Value).ToList();
            var totalNumberOfFeeds = feedsList.Count;

            var wrappedFeeds = feedsList.Wrap().Skip(currentRefreshIndex);

#if DEBUG
            await Task.Delay(5000);
#else
            await Task.Delay(highFrequencyRefreshPeriod);
#endif

            var fullPeriodInMs = RefreshPeriod.TotalMilliseconds;
            var pulseInterval = fullPeriodInMs / totalNumberOfFeeds;

            subHandle = Observable
                .Interval(TimeSpan.FromMilliseconds(pulseInterval))
                .Zip(wrappedFeeds, (_, feed) => feed)
                .Subscribe(feed =>
                {
                    RefreshFeed(feed);
                    IncrementCurrentIndex();
                }, 
                exception => { ; });
        }

        async void RefreshFeed(HighFrequencyFeed feed)
        {
            try
            {
                await feed.Refresh(CreateProcessor());
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }

        void IncrementCurrentIndex()
        {
            currentRefreshIndex++;
            currentRefreshIndex = currentRefreshIndex % feeds.Count;
        }

        IAsyncProcessor<HighFrequencyFeedUpdate> CreateProcessor()
        {
            return new StandardProcessorChain(connection);
        }

        public void Dispose()
        {
            if (subHandle != null)
                subHandle.Dispose();

            feeds = null;
        }
    }
}




#region unused

//async Task LoadAllFeeds()
//{
//    var feedsList = feeds.Select(o => o.Value).ToList();

//    var longTime = TimeSpan.FromMinutes(5);
//    foreach (var feed in feedsList)
//    {
//        feed.RefreshTimeout = longTime;
//    }

//    //foreach (var feed in feedsList)
//    //{
//    //    await feed.Refresh();
//    //}
//    await Task.WhenAll(feedsList.Select(o => o.Refresh()));

//    var shortTime = TimeSpan.FromMinutes(1);
//    foreach (var feed in feedsList)
//    {
//        feed.RefreshTimeout = shortTime;
//    }
//}

#endregion
