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
    public class HighFrequencyFeedUpdater
    {
        readonly string feedLibraryUrl;
        readonly ConnectionMultiplexer connection;
        readonly ProcessQueue<HighFrequencyFeed> processQueue;

        public TimeSpan RefreshPeriod { get; set; }




        #region Constructor

        public HighFrequencyFeedUpdater(
            string feedLibraryUrl, 
            ConnectionMultiplexer connection)
        {
            this.feedLibraryUrl = feedLibraryUrl;
            this.connection = connection;
            this.processQueue = new ProcessQueue<HighFrequencyFeed>();

            // set some default values
            RefreshPeriod = TimeSpan.FromMinutes(20);
        }

        #endregion




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

            foreach (var hff in highFrequencyFeeds)
            {
                processQueue.Enqueue(hff);
            }

#if DEBUG
            await RefreshAllFeedsImmediately();
#endif
        }




        #region Initialization helper functions

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

        async Task RefreshAllFeedsImmediately()
        {
            var feedsList = new List<HighFrequencyFeed>();
            var next = processQueue.GetNextFromQueue();
            while (next != null)
            {
                feedsList.Add(next);
                next = processQueue.GetNextFromQueue();
            }

            if (feedsList.Any())
            {
                await Task.WhenAll(feedsList.Select(o => o.Refresh(CreateProcessor())));
                foreach (var feed in feedsList)
                    processQueue.Requeue(feed);
            }
        }

        IAsyncProcessor<HighFrequencyFeedUpdate> CreateProcessor()
        {
            return new StandardProcessorChain(connection);
        }

        #endregion




        public async void StartFeedRefreshTimer()
        {
            var interval = TimeSpan.FromSeconds(2);

            while (true)
            {
                await Task.Delay(interval);
                ProcessNext();
            }
        }

        async void ProcessNext()
        {
            var next = processQueue.GetNextFromQueue();
            if (next == null)
                return;

            try
            {
                await next.Refresh(CreateProcessor());
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            processQueue.Requeue(next);
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
