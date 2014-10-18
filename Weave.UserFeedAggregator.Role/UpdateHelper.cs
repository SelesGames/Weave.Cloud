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
using Weave.User.Service.Redis.Clients;

namespace Weave.User.Service.Role.Controllers
{
    class UpdateHelper
    {
        readonly ConnectionMultiplexer connection;
        readonly TimingHelper th;

        public UpdateHelper(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            this.th = new TimingHelper();
        }

        public async Task<dynamic> PerformRefreshOnFeeds(IEnumerable<FeedIndex> indices)
        {
            if (EnumerableEx.IsNullOrEmpty(indices))
                throw new ArgumentException("PerformRefreshOnFeeds: indices cannot be empty");

            dynamic meta = new ExpandoObject();

            var urls = indices.Select(o => o.Uri).Distinct();

            // Send the request to the news aggregator server to update the specified feed urls, saving them in Redis

            th.Start();
            var results = await GetUpdateResults(urls);
            meta.RefreshServiceRoundtripTime = th.Record().Dump();
            meta.NewsServiceUpdateResults = results;

            var resultTuples = indices.Zip(results);

            // filter the list of urls to only one's that have been successfully updated
            var filteredIndices = resultTuples
                .Where(o => o.Item2.IsLoaded == true)
                .Select(o => o.Item1);
            urls = filteredIndices.Select(o => o.Uri);

            // Get the updater feeds from Redis - which should now have the most up-to-date info
            var cacheResult = await GetUpdaterFeeds(urls);
            meta.RedisFeedUpdaterResult_Deserialization = cacheResult.Timings.SerializationTime.Dump();
            meta.RedisFeedUpdaterResult_Retrieval = cacheResult.Timings.ServiceTime.Dump();

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
                "http://weave-v2-feedupdater.cloudapp.net/api/weave", urls.ToList());
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
            readonly DateTime now;

            public FeedIndexUpdateHelper(FeedIndex index, RedisCacheResult<Feed> updateResult)
            {
                this.index = index;
                this.updateResult = updateResult;
                this.now = DateTime.UtcNow;
            }

            public void UpdateFeedIndex()
            {
                //// copy the state of the update to the existing feed - ?? maybe not necessary??
                //var update = redisResult.Value;
                //update.CopyStateTo(feed);

                if (!updateResult.HasValue)
                    return;

                var feed = updateResult.Value;

                // you shouldn't need to update the Uri
                //index.Uri = feed.Uri;
                index.IconUri = feed.IconUri;
                index.TeaserImageUri = feed.TeaserImageUri;

                #region alternative approach to updating, using canonical
                //// should be safe to key off of Id, since the set determines uniqueness by ID
                //var userNewsItemIndicesLookup = index.NewsItemIndices.ToDictionary(o => o.Id);

                //var currentNewsSet = feed.News.Select(CreateNewIndexFromRecord);

                //index.NewsItemIndices.Clear();
                //foreach (var ni in currentNewsSet)
                //{
                //    var temp = ni;
                //    NewsItemIndex existing;
                //    if (userNewsItemIndicesLookup.TryGetValue(temp.Id, out existing))
                //    {
                //        temp.UtcPublishDateTime = existing.UtcPublishDateTime;
                //        temp.OriginalDownloadDateTime = existing.OriginalDownloadDateTime;
                //        temp.HasImage = existing.HasImage;
                //        temp.IsFavorite = existing.IsFavorite;
                //        temp.HasBeenViewed = existing.HasBeenViewed;
                //    }
                //    index.NewsItemIndices.Add(temp);
                //}
                #endregion

                var newsDiff = index.NewsItemIndices.Diff(feed.News, o => o.Id, o => o.Id);
                
                foreach (var newsItem in newsDiff.Removed.Where(o => !o.IsFavorite))
                {
                    index.NewsItemIndices.Remove(newsItem);
                }

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
                    OriginalDownloadDateTime = now,
                    HasImage = o.HasImage,
                    IsFavorite = false,
                    HasBeenViewed = false,
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