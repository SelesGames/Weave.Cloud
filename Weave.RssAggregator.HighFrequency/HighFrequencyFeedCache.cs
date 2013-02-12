using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedCache : IDisposable
    {
        Dictionary<string, HighFrequencyFeed> feeds = new Dictionary<string, HighFrequencyFeed>();
        CompositeDisposable disposables = new CompositeDisposable();

        int highFrequencyRefreshSplit;
        TimeSpan highFrequencyRefreshPeriod;




        #region Constructors

        public HighFrequencyFeedCache(string feedLibraryUrl)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            var feedClient = new FeedLibraryClient();
            var libraryFeeds = feedClient.GetFeeds(feedLibraryUrl);

            var highFrequencyFeeds = libraryFeeds
                .Distinct()
                .Select(
                    o => new HighFrequencyFeed
                    {
                        Name = o.Name,
                        FeedUri = o.Url,
                        IsDescriptionSuppressed = o.IsDescriptionSuppressed,
                    })
                .ToList();

            foreach (var feed in highFrequencyFeeds)
                feed.InitializeId();

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);
        }

        public HighFrequencyFeedCache(string feedLibraryUrl, int highFrequencyRefreshSplit, TimeSpan highFrequencyRefreshPeriod)
            : this(feedLibraryUrl)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            // set some default values
            this.highFrequencyRefreshPeriod = highFrequencyRefreshPeriod;
            this.highFrequencyRefreshSplit = highFrequencyRefreshSplit;

            StartFeedRefreshTimer();
        }

        #endregion




        void StartFeedRefreshTimer()
        {
            disposables.Clear();

            var feedsList = feeds.Select(o => o.Value).ToList();

            foreach (var feed in feedsList)
                feed.Refresh();

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

        public bool Contains(string feedUrl)
        {
            return feeds.ContainsKey(feedUrl);
        }

        //public HighFrequencyFeed Get(string feedUrl)
        //{
        //    return feeds[feedUrl];
        //}

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }
}
