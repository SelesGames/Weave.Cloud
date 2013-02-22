using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedUpdater : IDisposable
    {
        Dictionary<string, HighFrequencyFeed> feeds = new Dictionary<string, HighFrequencyFeed>();
    
        //int highFrequencyRefreshSplit;
        TimeSpan highFrequencyRefreshPeriod;
        string feedLibraryUrl;
        SequentialProcessor processor;
        int currentRefreshIndex = 0;
        IDisposable subHandle;



        #region Constructors

        public HighFrequencyFeedUpdater(string feedLibraryUrl, SequentialProcessor processor)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            this.feedLibraryUrl = feedLibraryUrl;
            this.processor = processor;
        }

        public HighFrequencyFeedUpdater(
                                    string feedLibraryUrl,
                                    SequentialProcessor processor, 
                                    int highFrequencyRefreshSplit, 
                                    TimeSpan highFrequencyRefreshPeriod)

            : this(feedLibraryUrl, processor)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

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
                .Select(o => new HighFrequencyFeed(o.FeedName, o.FeedUri))
                .ToList();

            foreach (var feed in highFrequencyFeeds)
            {
                if (processor != null)
                    processor.Register(feed);
            }

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);

            //await LoadAllFeeds();
        }

        async Task LoadAllFeeds()
        {
            var feedsList = feeds.Select(o => o.Value).ToList();

            var longTime = TimeSpan.FromMinutes(5);
            foreach (var feed in feedsList)
            {
                feed.RefreshTimeout = longTime;
            }

            //foreach (var feed in feedsList)
            //{
            //    await feed.Refresh();
            //}
            await Task.WhenAll(feedsList.Select(o => o.Refresh()));

            var shortTime = TimeSpan.FromMinutes(1);
            foreach (var feed in feedsList)
            {
                feed.RefreshTimeout = shortTime;
            }
        }

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

            //var indexedHFFeeds = feedsList.Select((hfFeed, i) => new { i, feed = hfFeed }).ToList();

            var fullPeriodInMs = highFrequencyRefreshPeriod.TotalMilliseconds;
            var pulseInterval = fullPeriodInMs / totalNumberOfFeeds;

            //var disp = Observable
            //    .Interval(TimeSpan.FromMilliseconds(pulseInterval))
            //    .Subscribe(
            //        i =>
            //        {
            //            var bucket = i % highFrequencyRefreshSplit;
            //            foreach (var indexedFeedUrl in indexedHFFeeds)
            //            {
            //                if (indexedFeedUrl.i % highFrequencyRefreshSplit == bucket)
            //                    indexedFeedUrl.feed.Refresh();
            //            }
            //        },
            //        exception => { ; });

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
