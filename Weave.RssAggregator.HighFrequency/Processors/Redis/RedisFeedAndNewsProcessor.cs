using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{ 
    /// <summary>
    /// Saves the updated Feed Index and added News Items to Redis, as a transaction
    /// </summary>
    public class RedisFeedAndNewsProcessor : ISequentialAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly ConnectionMultiplexer connection;

        public RedisFeedAndNewsProcessor(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            try
            {
                var db = connection.GetDatabase(DatabaseNumbers.CANONICAL_FEEDS_AND_NEWSITEMS);
                var transaction = db.CreateTransaction();
                var canonicalFeedCache = new CanonicalFeedIndexCache(transaction);
                var newsItemsCache = new NewsItemCache(transaction);

                var index = Map(update.InnerFeed);
                var newsItems = update.Entries.Select(Map);

                var feedResultTask = canonicalFeedCache.Save(index);
                var newsItemsResultsTask = newsItemsCache.Set(newsItems);

                await transaction.ExecuteAsync(flags: CommandFlags.None);
                var feedResult = await feedResultTask;
                var newsItemsResults = await newsItemsResultsTask;

                DebugEx.WriteLine(feedResult);
                DebugEx.WriteLine(newsItemsResults);
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine("\r\n\r\n**** RedisFeedAndNewsProcessor ERROR ****");
                DebugEx.WriteLine(ex);
            }
        }




        #region FeedIndex Map functions

        static FeedIndex Map(Feed o)
        {
            if (o == null)
                throw new ArgumentNullException("o in CreateCanonicalIndex should never be null");

            var feedIndex = new FeedIndex
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                TeaserImageUrl = o.TeaserImageUrl,
                LastRefreshedOn = o.LastRefreshedOn,
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
            };

            foreach (var entry in o.News)
            {
                var newsItemIndex = CreateIndex(entry);
                feedIndex.NewsItemIndices.Add(newsItemIndex);
            }

            return feedIndex;
        }

        static NewsItemIndex CreateIndex(ExpandedEntry o)
        {
            return new NewsItemIndex
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                HasImage = o.Images.Any(),
            };
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