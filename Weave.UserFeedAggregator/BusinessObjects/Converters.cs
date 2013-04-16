using SelesGames.Common;
using System.Linq;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;


namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class Converters :
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>,
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.Image, Image>,
        IConverter<NewsItem, Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem>,
        IConverter<Image, RssAggregator.Core.DTOs.Outgoing.Image>,
        IConverter<User.DataStore.Image, Image>,
        IConverter<User.DataStore.NewsItem, NewsItem>,
        IConverter<User.DataStore.Feed, Feed>,
        IConverter<User.DataStore.UserInfo, UserInfo>,
        IConverter<Image, User.DataStore.Image>,
        IConverter<NewsItem, User.DataStore.NewsItem>,
        IConverter<Feed, User.DataStore.Feed>,
        IConverter<UserInfo, User.DataStore.UserInfo>,
        IConverter<UserInfo, Outgoing.UserInfo>,
        IConverter<Feed, Outgoing.Feed>,
        IConverter<NewsItem, Outgoing.NewsItem>,
        IConverter<Image, Outgoing.Image>,
        IConverter<Incoming.NewFeed, Feed>,
        IConverter<Incoming.UpdatedFeed, Feed>,
        IConverter<Incoming.UserInfo, UserInfo>
    {
        public static readonly Converters Instance = new Converters();

        public Image Convert(RssAggregator.Core.DTOs.Outgoing.Image o)
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
                Image = o.Image == null ? null : o.Image.Convert<RssAggregator.Core.DTOs.Outgoing.Image, Image>(Instance),
            };
        }

        RssAggregator.Core.DTOs.Outgoing.Image IConverter<Image, RssAggregator.Core.DTOs.Outgoing.Image>.Convert(Image o)
        {
            return new RssAggregator.Core.DTOs.Outgoing.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        RssAggregator.Core.DTOs.Outgoing.NewsItem IConverter<NewsItem, RssAggregator.Core.DTOs.Outgoing.NewsItem>.Convert(NewsItem o)
        {
            return new RssAggregator.Core.DTOs.Outgoing.NewsItem
            {
                Id = o.Id,
                FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                PublishDateTime = o.UtcPublishDateTimeString,
                Description = null,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Image = o.Image == null ? null : o.Image.Convert<Image, RssAggregator.Core.DTOs.Outgoing.Image>(Instance),
            };
        }




        #region from Server Incoming to Business Objects

        public Feed Convert(Incoming.NewFeed o)
        {
            return new Feed
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public Feed Convert(Incoming.UpdatedFeed o)
        {
            return new Feed
            {
                Id = o.Id,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public UserInfo Convert(Incoming.UserInfo o)
        {
            var user = new UserInfo { Id = o.Id, };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<Incoming.NewFeed>().Select(x => x.Convert<Incoming.NewFeed, Feed>(Instance)))
                    user.AddFeed(feed, trustSource: false);
            }

            return user;
        }

        #endregion




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
                Uri = o.FeedUri,
                Name = o.FeedName,
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
            var user = new UserInfo 
            { 
                Id = o.Id,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
            };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<User.DataStore.Feed>().Select(x => x.Convert<User.DataStore.Feed, Feed>(Instance)))
                    user.AddFeed(feed, trustSource: true);
            }

            return user;
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
                FeedUri = o.Uri,
                FeedName = o.Name,
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
                FeedCount = o.Feeds == null ? 0 : o.Feeds.Count,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
            };
        }

        #endregion




        #region from Business Objects to Server Outgoing

        Outgoing.Image IConverter<Image, Outgoing.Image>.Convert(Image o)
        {
            return new Outgoing.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }
        
        Outgoing.NewsItem IConverter<NewsItem, Outgoing.NewsItem>.Convert(NewsItem o)
        {
            return new Outgoing.NewsItem
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
                Image = o.Image == null ? null : o.Image.Convert<Image, Outgoing.Image>(Instance),
            };
        }

        Outgoing.Feed IConverter<Feed, Outgoing.Feed>.Convert(Feed o)
        {
            return new Outgoing.Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (Weave.UserFeedAggregator.DTOs.ArticleViewingType)o.ArticleViewingType,
                News = o.News == null ? null : o.News.OfType<NewsItem>().Select(x => x.Convert<NewsItem, Outgoing.NewsItem>(Instance)).ToList(),
                TotalArticleCount = o.News == null ? 0 : o.News.Count,
            };
        }

        Outgoing.UserInfo IConverter<UserInfo, Outgoing.UserInfo>.Convert(UserInfo o)
        {
            return new Outgoing.UserInfo
            {
                Id = o.Id,
                FeedCount = o.Feeds == null ? 0 : o.Feeds.Count,
                Feeds = o.Feeds == null ? null : o.Feeds.OfType<Feed>().Select(x => x.Convert<Feed, Outgoing.Feed>(Instance)).ToList(),
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
            };
        }

        #endregion
    }
}
