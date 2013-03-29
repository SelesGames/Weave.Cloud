using SelesGames.Common;
using System.Linq;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class Converters : 
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>,
        IConverter<User.DataStore.Image, Image>,
        IConverter<User.DataStore.NewsItem, NewsItem>,
        IConverter<User.DataStore.Feed, Feed>,
        IConverter<User.DataStore.UserInfo, UserInfo>,
        IConverter<Image, User.DataStore.Image>,
        IConverter<NewsItem, User.DataStore.NewsItem>,
        IConverter<Feed, User.DataStore.Feed>,
        IConverter<UserInfo, User.DataStore.UserInfo>
    {
        public static readonly Converters Instance = new Converters();

        public NewsItem Convert(RssAggregator.Core.DTOs.Outgoing.NewsItem o)
        {
            return new NewsItem
            {
                Id = o.Id,
                FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                UtcPublishDateTimeString = o.PublishDateTime,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                HasBeenViewed = false,
                IsFavorite = false,
            };
        }




        #region From DataStore to Business Objects
        
        public Image Convert(User.DataStore.Image o)
        {
            return new Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        public NewsItem Convert(User.DataStore.NewsItem o)
        {
            return new NewsItem
            {
                Id = o.Id,
                FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                UtcPublishDateTimeString = o.UtcPublishDateTime,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                HasBeenViewed = o.HasBeenViewed,
                IsFavorite = o.IsFavorite, 
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                Image = o.Image == null ? null : o.Image.Convert<User.DataStore.Image, Image>(Instance),
            };
        }

        public Feed Convert(User.DataStore.Feed o)
        {
            return new Feed
            {
                Id = o.Id,
                FeedUri = o.FeedUri,
                FeedName = o.FeedName,
                Category = o.Category,
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                LastRefreshedOn = o.LastRefreshedOn,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
                News = o.News == null ? null : o.News.OfType<User.DataStore.NewsItem>().Select(x => x.Convert<User.DataStore.NewsItem, NewsItem>(Instance)).ToList(),
            };
        }

        public UserInfo Convert(User.DataStore.UserInfo o)
        {
            return new UserInfo
            {
                Id = o.Id,
                Feeds = o.Feeds == null ? null : o.Feeds.OfType<User.DataStore.Feed>().Select(x => x.Convert<User.DataStore.Feed, Feed>(Instance)).ToList(),
            };
        }

        #endregion




        #region from Business Objects to Datastore

        public User.DataStore.Image Convert(Image o)
        {
            return new User.DataStore.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        public User.DataStore.NewsItem Convert(NewsItem o)
        {
            return new User.DataStore.NewsItem
            {
                Id = o.Id,
                FeedId = o.FeedId,
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
                Image = o.Image == null ? null : o.Image.Convert<Image, User.DataStore.Image>(Instance),
            };
        }

        public User.DataStore.Feed Convert(Feed o)
        {
            return new User.DataStore.Feed
            {
                Id = o.Id,
                FeedUri = o.FeedUri,
                FeedName = o.FeedName,
                Category = o.Category,
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                LastRefreshedOn = o.LastRefreshedOn,
                ArticleViewingType = (User.DataStore.ArticleViewingType)o.ArticleViewingType,
                News = o.News == null ? null : o.News.OfType<NewsItem>().Select(x => x.Convert<NewsItem, User.DataStore.NewsItem>(Instance)).ToList(),
            };
        }

        public User.DataStore.UserInfo Convert(UserInfo o)
        {
            return new User.DataStore.UserInfo
            {
                Id = o.Id,
                Feeds = o.Feeds == null ? null : o.Feeds.OfType<Feed>().Select(x => x.Convert<Feed, User.DataStore.Feed>(Instance)).ToList(),
            };
        }

        #endregion
    }
}
