using SelesGames.Common;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects;
using Store = Weave.User.DataStore;

namespace Weave.User.Service.Converters
{
    public class DataStoreToBusinessObject :
        IConverter<Store.Image, Image>,
        IConverter<Store.NewsItem, NewsItem>,
        IConverter<Store.Feed, Feed>,
        IConverter<Store.UserInfo, UserInfo>
    {
        public static readonly DataStoreToBusinessObject Instance = new DataStoreToBusinessObject();

        public UserInfo Convert(Store.UserInfo o)
        {
            var user = new UserInfo
            {
                Id = o.Id,
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
            };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<Store.Feed>().Select(Convert))
                    user.AddFeed(feed, trustSource: true);
            }

            return user;
        }

        public Feed Convert(Store.Feed o)
        {
            var feed = new Feed
            {
                Id = o.Id,
                Uri = o.FeedUri,
                Name = o.FeedName,
                IconUri = o.IconUri,
                Category = o.Category,
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                LastRefreshedOn = o.LastRefreshedOn,
                PreviousEntrance = o.PreviousEntrance,
                MostRecentEntrance = o.MostRecentEntrance,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
                //News = o.News == null ? null : o.News.OfType<Store.NewsItem>().Select(Convert).ToList(),
            };

            feed.News = o.News == null ? null : GetJoinedNews(new[] { feed }, o.News.OfType<Store.NewsItem>().Select(Convert).ToList()).ToList();
            return feed;
        }

        public NewsItem Convert(Store.NewsItem o)
        {
            return new NewsItem
            {
                Id = o.Id,
                //FeedId = o.FeedId,
                Feed = new Feed { Id = o.FeedId },
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
                Image = o.Image == null ? null : Convert(o.Image),
            };
        }

        public Image Convert(Store.Image o)
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

        IEnumerable<NewsItem> GetJoinedNews(IEnumerable<Feed> feeds, IEnumerable<NewsItem> news)
        {
            return from n in news
                   join f in feeds on n.Feed.Id equals f.Id
                   select Convert(n, f);
        }

        NewsItem Convert(NewsItem n, Feed f)
        {
            n.Feed = f;
            return n;
        }

        //public UserNewsItemState Convert(Store.UserNewsItemState o)
        //{
        //    return new UserNewsItemState(o.Id, o.NewsItemStates == null ? null : o.NewsItemStates.Select(Convert));
        //}

        //public NewsItemState Convert(Store.NewsItemState o)
        //{
        //    return new NewsItemState
        //    {
        //        Id = o.Id,
        //        HasBeenViewed = o.HasBeenViewed,
        //        IsFavorite = o.IsFavorite,
        //    };
        //}
    }
}
