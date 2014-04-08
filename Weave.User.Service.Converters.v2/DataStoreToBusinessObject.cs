using SelesGames.Common;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects.v2;
using Store = Weave.User.DataStore.v2;

namespace Weave.User.Service.Converters.v2
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
                    user.Feeds.TryAdd(feed, trustSource: true);
            }

            return user;
        }

        public Feed Convert(Store.Feed o)
        {
            return new Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                Etag = o.Etag,
                LastModified = o.LastModified,
                MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate,
                LastRefreshedOn = o.LastRefreshedOn,
                PreviousEntrance = o.PreviousEntrance,
                MostRecentEntrance = o.MostRecentEntrance,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
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
    }
}