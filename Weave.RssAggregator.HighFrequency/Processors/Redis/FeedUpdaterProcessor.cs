using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;
using System.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    /// <summary>
    /// Saves the state of the Updater Feed
    /// </summary>
    public class FeedUpdaterProcessor : ISequentialAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly ConnectionMultiplexer connection;

        public FeedUpdaterProcessor(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            try
            {
                var saveFeedResult = await SaveFeed(update.InnerFeed);
                DebugEx.WriteLine("Took {0} ms to serialize, {1} ms to save updater feed for {2}", saveFeedResult.Timings.SerializationTime.TotalMilliseconds, saveFeedResult.Timings.ServiceTime.TotalMilliseconds, update.Feed.Name);

                var saveNewsResults = await SaveNewNews(update);
                DebugEx.WriteLine("Took {0} ms to serialize, {1} ms to save news for {2}", saveNewsResults.Timings.SerializationTime.TotalMilliseconds, saveNewsResults.Timings.ServiceTime.TotalMilliseconds, update.Feed.Name);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("\r\n\r\n**** FeedUpdaterProcessor ERROR ****");
                DebugEx.WriteLine(ex);
            }
        }




        #region Redis Write functions

        Task<RedisWriteResult<bool>> SaveFeed(Feed feed)
        {
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var cache = new FeedUpdaterCache(db);
            return cache.Save(feed);
        }

        async Task<RedisWriteMultiResult<bool>> SaveNewNews(HighFrequencyFeedUpdate update)
        {
            if (update.Entries == null || !update.Entries.Any())
                return null;

            var news = update.Entries.Select(Map);

            var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_FEEDS_AND_NEWSITEMS);
            var batch = db.CreateBatch();
            var newsCache = new NewsItemCache(batch);

            var resultTask = newsCache.Set(news);

            batch.Execute();

            var result = await resultTask;
            return result;
        }

        #endregion




        #region NewsItem Map functions

        static Weave.User.Service.Redis.DTOs.NewsItem Map(ExpandedEntry o)
        {
            var bestImage = o.Images.GetBest();

            return new Weave.User.Service.Redis.DTOs.NewsItem
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                UtcPublishDateTimeString = o.UtcPublishDateTimeString,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = bestImage == null ? null : bestImage.Url,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Image = bestImage == null ? null : Map(bestImage),
            };
        }

        static Weave.User.Service.Redis.DTOs.Image Map(Image o)
        {
            return new Weave.User.Service.Redis.DTOs.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.Url,
                BaseImageUrl = null,
                SupportedFormats = null,
            };
        }

        #endregion
    }
}