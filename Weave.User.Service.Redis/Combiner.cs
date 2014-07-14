using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis
{
    public class Combiner
    {
        UserIndex user;
        NewsItemCache news;

        public Combiner(UserIndex user, NewsItemCache news)
        {
            this.user = user;
            this.news = news;
        }

        public async Task<IEnumerable<NewsItem>> GetNews(
            Func<FeedIndex, bool> feedFilter,
            Guid userId, 
            Guid feedId, 
            int skip, 
            int take, 
            Func<NewsItemIndex, bool> newsFilter = null)//, NewsItemType type = NewsItemType.Any, bool requireImage = false)
        {
            var feeds = user
                .FeedIndices
                .Where(feedFilter);

            var indices = feeds.Ordered().ToArray();

            IEnumerable<NewsItemIndex> filteredIndices = indices;
            if (newsFilter != null)
                filteredIndices = filteredIndices.Where(newsFilter);

            filteredIndices = filteredIndices.Skip(skip).Take(take).ToList();

            var newsItems = await news.Get(indices.Select(o => o.Id));

            var zipped = filteredIndices.Zip(newsItems, (index, ni) => new { index, ni });

            var results = 
                from temp in zipped.Where(o => o.ni.Value != null)
                let newsIndex = temp.index
                let newsItem = temp.ni.Value
                select Merge(newsIndex, newsItem);

            return results;
        }

        static NewsItem Merge(NewsItemIndex newsIndex, DTOs.NewsItem newsItem)
        {
            return new NewsItem
            {
                Id = newsItem.Id,
                Title = newsItem.Title,
                Link = newsItem.Link,
                ImageUrl = newsItem.ImageUrl,
                YoutubeId = newsItem.YoutubeId,
                VideoUri = newsItem.VideoUri,
                PodcastUri = newsItem.PodcastUri,
                ZuneAppId = newsItem.ZuneAppId,
                IsFavorite = newsIndex.IsFavorite,
                HasBeenViewed = newsIndex.HasBeenViewed,
                OriginalDownloadDateTime = newsItem.OriginalDownloadDateTime,
                UtcPublishDateTimeString = newsItem.UtcPublishDateTime,
                Image = newsItem.Image == null ? null :
                    new Image
                    {
                        Width = newsItem.Image.Width,
                        Height = newsItem.Image.Height,
                        OriginalUrl = newsItem.Image.OriginalUrl,
                        BaseImageUrl = newsItem.Image.BaseImageUrl,
                        SupportedFormats = newsItem.Image.SupportedFormats,
                    }
            };
        }
    }

    public static class CombinerExtensions
    {
        public static IEnumerable<NewsItemIndex> Ordered(this IEnumerable<FeedIndex> feeds)
        {
            return feeds
                .Where(o => o.NewsItemIndices != null)
                .SelectMany(o => o.NewsItemIndices)
                .OrderByDescending(o => o.UtcPublishDateTime);
        }
    }
}
