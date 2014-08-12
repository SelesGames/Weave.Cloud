using System;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    public class RedisCanonicalFeedIndexProcessor : ISequentialAsyncProcessor<FeedUpdate>
    {
        CanonicalFeedIndexCache cache;

        public RedisCanonicalFeedIndexProcessor(CanonicalFeedIndexCache cache)
        {
            this.cache = cache;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(FeedUpdate update)
        {
            try
            {
                var index = Map(update.Feed);
                var wasSaved = await cache.Save(index);
            }
            catch { }
        }




        #region Map functions

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
                var newsItemIndex = Map(entry);
                feedIndex.NewsItemIndices.Add(newsItemIndex);
            }

            return feedIndex;
        }

        static NewsItemIndex Map(ExpandedEntry o)
        {
            return new NewsItemIndex
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                HasImage = o.Images.GetBest() != null,
            };
        }

        #endregion
    }
}
