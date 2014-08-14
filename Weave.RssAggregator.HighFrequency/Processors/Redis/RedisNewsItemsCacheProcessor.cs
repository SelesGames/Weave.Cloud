using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    /// <summary>
    /// Will add the full canonical info for the articles to the news items cache,
    /// and will also add the news item Ids/Score tuple to the sorted news item
    /// index for a particular feed
    /// </summary>
    public class RedisNewsItemsCacheProcessor : ISequentialAsyncProcessor<FeedUpdate>
    {
        NewsItemCache adder;

        public RedisNewsItemsCacheProcessor(NewsItemCache adder)
        {
            this.adder = adder;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(FeedUpdate update)
        {
            try
            {
                var entries = update.Entries ?? new List<ExpandedEntry>();
                var news = entries.Select(Map).ToList();

                var feedId = update.Feed.Id;

                await adder.AddNews(feedId, news);
            }
            catch { }
        }




        #region Map functions

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
