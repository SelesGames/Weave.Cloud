//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Weave.Updater.BusinessObjects;
//using Weave.User.BusinessObjects.Mutable;

//namespace Weave.User.Service.Redis
//{
//    public static class FeedUpdateRedisExtensions
//    {
//        public static async Task<SaveFeedIndexAndNewNewsResult> SaveFeedIndexAndNewNews(this IDatabaseAsync db, FeedUpdate update)
//        {
//            var canonicalFeedCache = new CanonicalFeedIndexCache(db);
//            var newsItemsCache = new NewsItemCache(db);

//            var index = Map(update.Feed);
//            var newsItems = update.Entries.Select(Map);

//            var feedResultTask = canonicalFeedCache.Save(index);
//            var newsItemsResultsTask = newsItemsCache.Set(newsItems);

//            var feedResult = await feedResultTask;
//            var newsItemsResults = await newsItemsResultsTask;

//            return new SaveFeedIndexAndNewNewsResult { FeedIndexSaveResult = feedResult, NewsItemsSaveResult = newsItemsResults };
//        }




//        #region FeedIndex Map functions

//        static FeedIndex Map(Feed o)
//        {
//            if (o == null)
//                throw new ArgumentNullException("o in CreateCanonicalIndex should never be null");

//            var feedIndex = new FeedIndex
//            {
//                Id = o.Id,
//                Uri = o.Uri,
//                Name = o.Name,
//                TeaserImageUrl = o.TeaserImageUrl,
//                LastRefreshedOn = o.LastRefreshedOn,
//                Etag = o.Etag,
//                LastModified = o.LastModified,
//                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
//            };

//            foreach (var entry in o.News)
//            {
//                var newsItemIndex = CreateIndex(entry);
//                feedIndex.NewsItemIndices.Add(newsItemIndex);
//            }

//            return feedIndex;
//        }

//        static NewsItemIndex CreateIndex(ExpandedEntry o)
//        {
//            return new NewsItemIndex
//            {
//                Id = o.Id,
//                UtcPublishDateTime = o.UtcPublishDateTime,
//                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
//                HasImage = o.Images.Any(),
//            };
//        }

//        #endregion




//        #region NewsItem Map functions

//        static Weave.User.Service.Redis.DTOs.NewsItem Map(ExpandedEntry o)
//        {
//            var bestImage = o.Images.GetBest();

//            return new Weave.User.Service.Redis.DTOs.NewsItem
//            {
//                Id = o.Id,
//                UtcPublishDateTime = o.UtcPublishDateTime,
//                UtcPublishDateTimeString = o.UtcPublishDateTimeString,
//                Title = o.Title,
//                Link = o.Link,
//                ImageUrl = bestImage == null ? null : bestImage.Url,
//                YoutubeId = o.YoutubeId,
//                VideoUri = o.VideoUri,
//                PodcastUri = o.PodcastUri,
//                ZuneAppId = o.ZuneAppId,
//                Image = bestImage == null ? null : Map(bestImage),
//            };
//        }

//        static Weave.User.Service.Redis.DTOs.Image Map(Image o)
//        {
//            return new Weave.User.Service.Redis.DTOs.Image
//            {
//                Width = o.Width,
//                Height = o.Height,
//                OriginalUrl = o.Url,
//                BaseImageUrl = null,
//                SupportedFormats = null,
//            };
//        }

//        #endregion
//    }

//    public class SaveFeedIndexAndNewNewsResult
//    {
//        public RedisWriteResult<bool> FeedIndexSaveResult { get; set; }
//        public IEnumerable<bool> NewsItemsSaveResult { get; set; }
//    }
//}
