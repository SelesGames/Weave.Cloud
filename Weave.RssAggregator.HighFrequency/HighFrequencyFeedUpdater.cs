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

        readonly TimeSpan highFrequencyRefreshPeriod;
        readonly string feedLibraryUrl;
        readonly SequentialProcessor processor;
        readonly IDatabase db;
        int currentRefreshIndex = 0;
        IDisposable subHandle;



        #region Constructors

        public HighFrequencyFeedUpdater(
            string feedLibraryUrl, 
            SequentialProcessor processor, 
            ConnectionMultiplexer connection)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            this.feedLibraryUrl = feedLibraryUrl;
            this.processor = processor;
            this.db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);

            // set some default values
            this.highFrequencyRefreshPeriod = TimeSpan.FromMinutes(20);
        }

        public HighFrequencyFeedUpdater(
            string feedLibraryUrl,
            SequentialProcessor processor,
            ConnectionMultiplexer connection,
            TimeSpan highFrequencyRefreshPeriod)

            : this(feedLibraryUrl, processor, connection)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            this.highFrequencyRefreshPeriod = highFrequencyRefreshPeriod;
        }

        #endregion




        public async Task InitializeAsync()
        {
            var feedClient = new FeedLibraryClient(feedLibraryUrl);

            await feedClient.LoadFeedsAsync();
            var libraryFeeds = feedClient.Feeds;

            var highFrequencyFeedsTasks = libraryFeeds
                .Distinct()
                //.Where(o => !string.IsNullOrWhiteSpace(o.CorrectedUri))
                //.Where(o => o.FeedUri == "http://feeds.feedburner.com/Destructoid")
                .Select(CreateHighFrequencyFeed);

            IEnumerable<HighFrequencyFeed> highFrequencyFeeds = 
                await Task.WhenAll(highFrequencyFeedsTasks);

            highFrequencyFeeds = highFrequencyFeeds.OfType<HighFrequencyFeed>();

            foreach (var feed in highFrequencyFeeds)
            {
                if (processor != null)
                    processor.Register(feed);
            }

            feeds = highFrequencyFeeds.ToDictionary(o => o.Uri);
        }

        async Task<HighFrequencyFeed> CreateHighFrequencyFeed(FeedSource o)
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

                var hff = new HighFrequencyFeed(o.FeedName, feedUri, originalUri, o.Instructions, db);
                await hff.Initialize();
                return hff;
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                return null;
            }
        }


#if DEBUG
        public Task RefreshAllFeedsImmediately()
        {
            var feedsList = feeds.Select(o => o.Value).ToList();
            return Task.WhenAll(feedsList.Select(o => o.Refresh()));
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

            var fullPeriodInMs = highFrequencyRefreshPeriod.TotalMilliseconds;
            var pulseInterval = fullPeriodInMs / totalNumberOfFeeds;

            subHandle = Observable
                .Interval(TimeSpan.FromMilliseconds(pulseInterval))
                .Zip(wrappedFeeds, (_, feed) => feed)
                .Subscribe(feed =>
                {
                    feed.Refresh();
                    IncrementCurrentIndex();
                }, 
                exception => { ; });
        }

        void IncrementCurrentIndex()
        {
            currentRefreshIndex++;
            currentRefreshIndex = currentRefreshIndex % feeds.Count;
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
