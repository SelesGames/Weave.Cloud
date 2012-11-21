using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedRssCache : IDisposable
    {
        Dictionary<string, HighFrequencyFeed> feeds = new Dictionary<string, HighFrequencyFeed>();
        CompositeDisposable disposables = new CompositeDisposable();

        public int HighFrequencyRefreshSplit { get; private set; }
        public TimeSpan HighFrequencyRefreshPeriod { get; private set; }

        public HighFrequencyFeedRssCache(string feedLibraryUrl, int highFrequencyRefreshSplit, TimeSpan highFrequencyRefreshPeriod)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl))
                return;

            // set some default values
            HighFrequencyRefreshPeriod = highFrequencyRefreshPeriod;
            HighFrequencyRefreshSplit = highFrequencyRefreshSplit;

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
                feed.Refresh();

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);

            var indexedHFFeeds = highFrequencyFeeds.Select((hfFeed, i) => new { i, feed = hfFeed }).ToList();

            var fullPeriodInMs = HighFrequencyRefreshPeriod.TotalMilliseconds;
            var splitInterval = fullPeriodInMs / (double)HighFrequencyRefreshSplit;

            var disp = Observable
                .Interval(TimeSpan.FromMilliseconds(splitInterval))
                .Subscribe(
                    i =>
                    {
                        var bucket = i % HighFrequencyRefreshSplit;
                        foreach (var indexedFeedUrl in indexedHFFeeds)
                        {
                            if (indexedFeedUrl.i % HighFrequencyRefreshSplit == bucket)
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

        public HighFrequencyFeed Get(string feedUrl)
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
