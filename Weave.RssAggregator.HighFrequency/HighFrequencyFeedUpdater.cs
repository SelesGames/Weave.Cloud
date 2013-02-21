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
        CompositeDisposable disposables = new CompositeDisposable();

        int highFrequencyRefreshSplit;
        TimeSpan highFrequencyRefreshPeriod;
        string feedLibraryUrl;
        SequentialProcessor processor;



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
        }

        public async void StartFeedRefreshTimer()
        {
            disposables.Clear();

            var feedsList = feeds.Select(o => o.Value).ToList();

            //foreach (var feed in feedsList)
            //    feed.Refresh();

            await Task.Delay(5000);

            var indexedHFFeeds = feedsList.Select((hfFeed, i) => new { i, feed = hfFeed }).ToList();

            var fullPeriodInMs = highFrequencyRefreshPeriod.TotalMilliseconds;
            var splitInterval = fullPeriodInMs / (double)highFrequencyRefreshSplit;

            var disp = Observable
                .Interval(TimeSpan.FromMilliseconds(splitInterval))
                .Subscribe(
                    i =>
                    {
                        var bucket = i % highFrequencyRefreshSplit;
                        foreach (var indexedFeedUrl in indexedHFFeeds)
                        {
                            if (indexedFeedUrl.i % highFrequencyRefreshSplit == bucket)
                                indexedFeedUrl.feed.Refresh();
                        }
                    },
                    exception => { ; });

            disposables.Add(disp);
        }

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }
}
