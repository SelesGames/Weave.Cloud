using System;
using System.Collections;
using System.Collections.Concurrent;
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

        public HighFrequencyFeedCache(string feedLibraryUrl, SqlUpdater sqlUpdater)
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

            QueueConnector.Initialize();
            var serviceBusUpdater = new ServiceBusUpdater(QueueConnector.OrdersQueueClient);

            foreach (var feed in highFrequencyFeeds)
            {
                feed.InitializeId();
                //sqlUpdater.Register(feed);
                //serviceBusUpdater.Register(feed);
            }

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);
        }

        public HighFrequencyFeedCache(string feedLibraryUrl, SqlUpdater sqlUpdater, int highFrequencyRefreshSplit, TimeSpan highFrequencyRefreshPeriod)
            : this(feedLibraryUrl, sqlUpdater)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            // set some default values
            this.highFrequencyRefreshPeriod = highFrequencyRefreshPeriod;
            this.highFrequencyRefreshSplit = highFrequencyRefreshSplit;
        }

        #endregion




        public void StartFeedRefreshTimer()
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

        public HighFrequencyFeed GetFeedByUrl(string url)
        {
            HighFrequencyFeed feed = null;

            if (feeds.ContainsKey(url))
            {
                feed = feeds[url];
            }

            return feed;
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

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }



    public abstract class ThreadSafeDelegatingCollection<TKey, T> : ICollection<T>
    {
        ConcurrentDictionary<TKey, T> lookup;

        protected abstract void AddInternal(T item);
        protected abstract void RemoveInternal(T item);
        protected abstract TKey GetKey(T item);

        public void Add(T item)
        {
            var key = GetKey(item);

            lookup.AddOrUpdate(key, item, (k, i) => i);
        }

        public void Clear()
        {
            lookup.Clear();
        }

        public bool Contains(T item)
        {
            var key = GetKey(item);

            return lookup.ContainsKey(key);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return lookup.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            T val;
            var key = GetKey(item);

            return lookup.TryRemove(key, out val);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return lookup.Select(o => o.Value).GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lookup.Select(o => o.Value).GetEnumerator();
        }
    }
}
