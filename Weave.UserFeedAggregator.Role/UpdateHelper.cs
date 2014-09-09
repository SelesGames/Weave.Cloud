using SelesGames.HttpClient;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis;

namespace Weave.User.Service.Role.Controllers
{
    class UpdateHelper
    {
        readonly ConnectionMultiplexer connection;

        public UpdateHelper(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<dynamic> PerformRefreshOnFeeds(IEnumerable<FeedIndex> indices)
        {
            dynamic meta = new ExpandoObject();

            var urls = indices.Select(o => o.Uri).Distinct();

            // Send the request to the news aggregator server to update the specified feed urls, saving them in Redis
            var results = await GetUpdateResults(urls);
            meta.NewsServiceUpdateResults = results;

            var resultTuples = indices.Zip(results);

            // filter the list of urls to only one's that have been successfully updated
            var filteredIndices = resultTuples
                .Where(o => o.Item2.IsLoaded == true)
                .Select(o => o.Item1);
            urls = filteredIndices.Select(o => o.Uri);

            // Get the updater feeds from Redis - which should now have the most up-to-date info
            var cacheResult = await GetUpdaterFeeds(urls);
            meta.RedisFeedUpdaterResultTimings = cacheResult.Timings;

            foreach (var tuple in filteredIndices.Zip(cacheResult.Results))
            {
                var index = tuple.Item1;
                var feed = tuple.Item2;
                index.UpdateFrom(feed);
            }

            return meta;
        }

        Task<List<Result>> GetUpdateResults(IEnumerable<string> urls)
        {
            var client = new SmartHttpClient(CompressionSettings.None) { Timeout = TimeSpan.FromMinutes(2) };
            return client.PostAsync<List<string>, List<Result>>(
                "http://weave-v2-news.cloudapp.net/api/weave", urls.ToList());
        }

        Task<RedisCacheMultiResult<Feed>> GetUpdaterFeeds(IEnumerable<string> urls)
        {
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);
            return cache.Get(urls);
        }
    }

    static class FeedIndexUpdateExtensions
    {
        public static void UpdateFrom(this FeedIndex index, RedisCacheResult<Feed> updateResult)
        {
            var helper = new FeedIndexUpdateHelper(index, updateResult);
            helper.UpdateFeedIndex();
        }

        private class FeedIndexUpdateHelper
        {
            readonly FeedIndex index;
            readonly RedisCacheResult<Feed> updateResult;

            public FeedIndexUpdateHelper(FeedIndex index, RedisCacheResult<Feed> updateResult)
            {
                this.index = index;
                this.updateResult = updateResult;
            }

            public void UpdateFeedIndex()
            {
                //// copy the state of the update to the existing feed - ?? maybe not necessary??
                //var update = redisResult.Value;
                //update.CopyStateTo(feed);

                if (!updateResult.HasValue)
                    return;

                var feed = updateResult.Value;

                // you shouldn't need to update the Uri, and the IconUri is currently broken
                //index.Uri = feed.Uri;
                //index.IconUri = feed.IconUri;
                index.TeaserImageUrl = feed.TeaserImageUrl;

                var newsDiff = index.NewsItemIndices.Diff(feed.News, o => o.Id, o => o.Id);
                // don't remove a news item just because it was removed from the underlying feed!
                //foreach (var newsItem in newsDiff.Removed)
                //{
                //    index.NewsItemIndices.Remove(newsItem);
                //}

                foreach (var record in newsDiff.Added)
                {
                    var newsItemIndex = CreateNewIndexFromRecord(record);
                    index.NewsItemIndices.Add(newsItemIndex);
                }
            }

            NewsItemIndex CreateNewIndexFromRecord(Updater.BusinessObjects.NewsItemRecord o)
            {
                return new NewsItemIndex
                {
                    Id = o.Id,
                    UtcPublishDateTime = o.UtcPublishDateTime,
                    OriginalDownloadDateTime = DateTime.UtcNow,
                    HasImage = o.HasImage,
                    //FeedIndex = index,
                };
            }
        }
    }

    static class IEnumerableExtensions
    {
        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> one, 
            IEnumerable<T2> two)
        {
            return one.Zip(two, (i, j) => Tuple.Create(i, j));
        }
    }
}