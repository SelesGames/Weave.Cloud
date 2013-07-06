using SelesGames.Common;
using System.Linq;
using Weave.User.BusinessObjects;
using Store = Weave.User.DataStore;

namespace Weave.UserFeedAggregator.Converters
{
    public class BusinessObjectToDataStore :
        IConverter<Image, Store.Image>,
        IConverter<NewsItem, Store.NewsItem>,
        IConverter<Feed, Store.Feed>,
        IConverter<UserInfo, Store.UserInfo>
    {
        public static readonly BusinessObjectToDataStore Instance = new BusinessObjectToDataStore();

        public Store.UserInfo Convert(UserInfo o)
        {
            return new Store.UserInfo
            {
                Id = o.Id,
                Feeds = o.Feeds == null ? null : o.Feeds.OfType<Feed>().Select(Convert).ToList(),
                FeedCount = o.Feeds == null ? 0 : o.Feeds.Count,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
            };
        }

        public Store.Feed Convert(Feed o)
        {
            return new Store.Feed
            {
                Id = o.Id,
                FeedUri = o.Uri,
                FeedName = o.Name,
                Category = o.Category,
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                LastRefreshedOn = o.LastRefreshedOn,
                PreviousEntrance = o.PreviousEntrance,
                MostRecentEntrance = o.MostRecentEntrance,
                ArticleViewingType = (Store.ArticleViewingType)o.ArticleViewingType,
                News = o.News == null ? null : o.News.OfType<NewsItem>().Select(Convert).ToList(),
            };
        }

        public Store.NewsItem Convert(NewsItem o)
        {
            return new Store.NewsItem
            {
                Id = o.Id,
                FeedId = o.Feed.Id,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                UtcPublishDateTime = o.UtcPublishDateTimeString,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                HasBeenViewed = o.HasBeenViewed,
                IsFavorite = o.IsFavorite,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                Image = o.Image == null ? null : Convert(o.Image)
            };
        }

        public Store.Image Convert(Image o)
        {
            return new Store.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }
    }
}
