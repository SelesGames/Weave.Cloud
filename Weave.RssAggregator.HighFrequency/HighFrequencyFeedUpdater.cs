using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.LibraryClient;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedUpdater
    {
        readonly string feedLibraryUrl;
        readonly ProcessQueue<HighFrequencyFeed> processQueue;




        #region Constructor

        public HighFrequencyFeedUpdater(string feedLibraryUrl)
        {
            this.feedLibraryUrl = feedLibraryUrl;
            this.processQueue = new ProcessQueue<HighFrequencyFeed>();
        }

        #endregion




        public async Task InitializeAsync()
        {
            var feedClient = new FeedLibraryClient(feedLibraryUrl);

            await feedClient.LoadFeedsAsync();
            var libraryFeeds = feedClient.Feeds;

            var highFrequencyFeeds = libraryFeeds
                .Distinct()
                //.Where(o => o.FeedUri == "http://bleacherreport.com/articles;feed")
                .Select(CreateHighFrequencyFeed)
                .OfType<HighFrequencyFeed>()
                .ToList();

            await RecoverFeedStateForFeeds(highFrequencyFeeds);

            foreach (var hff in highFrequencyFeeds)
            {
                processQueue.Enqueue(hff);
            }

#if DEBUG
            await RefreshAllFeedsImmediately();
            await Task.Delay(TimeSpan.FromSeconds(30));
#endif

            StartFeedRefreshTimer();
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

        async Task RecoverFeedStateForFeeds(IEnumerable<HighFrequencyFeed> feeds)
        {
            var feedUrls = feeds.Select(o => o.Uri);

            var connection = Settings.StandardConnection;
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);
            var cacheMultiGet = await cache.Get(feedUrls);

            var recoveredFeedUpdaters = cacheMultiGet.GetValidValues().ToList();

            var joined = feeds.Join(
                recoveredFeedUpdaters,
                o => o.Uri.ToLowerInvariant(),
                o => o.Uri.ToLowerInvariant(),
                (hff, updater) => new { hff, updater });

            foreach (var tuple in joined)
            {
                tuple.hff.InitializeWith(tuple.updater);
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
                foreach (var feed in feedsList)
                {
                    await feed.Refresh(CreateProcessor());
                    processQueue.Requeue(feed);
                }
                //await Task.WhenAll(feedsList.Select(o => o.Refresh(CreateProcessor())));
                //foreach (var feed in feedsList)
                //    processQueue.Requeue(feed);
            }
        }

        IAsyncProcessor<HighFrequencyFeedUpdate> CreateProcessor()
        {
            return new StandardProcessorChain();
        }

        #endregion




        async void StartFeedRefreshTimer()
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
                DebugEx.WriteLine("{0} faulted: {1}", next, ex);
            }

            processQueue.Requeue(next);
        }
    }
}