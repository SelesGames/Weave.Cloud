using System;
using System.Threading.Tasks;
using Weave.Parsing;
using Weave.RssAggregator.HighFrequency;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequenc
{
    public class RedisCanonicalFeedIndexProcessor : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        CanonicalFeedIndexCache cache;

        public RedisCanonicalFeedIndexProcessor(CanonicalFeedIndexCache cache)
        {
            this.cache = cache;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            try
            {
                var index = Map(update.Feed);
                var wasSaved = await cache.Save(index);
            }
            catch { }
        }




        #region Map functions

        static FeedIndex Map(HighFrequencyFeed o)
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
        }

        static NewsItemIndex Map(Entry o)
        {
            return new NewsItemIndex
            {
                Id = o.Id,
                UrlHash = z,
                TitleHash = z,
                UtcPublishDateTime = o.UtcPublishDateTime,
                OriginalDownloadDateTime = o.o,
                HasImage = o.GetImageUrl(),
            };
        }

        #endregion
    }
}
