﻿using SelesGames.Common;
using System.Linq;
using Weave.User.BusinessObjects.v2;
using Store = Weave.User.DataStore.v2;

namespace Weave.User.Service.Converters.v2
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
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
            };
        }

        public Store.Feed Convert(Feed o)
        {
            return new Store.Feed
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
                ArticleViewingType = (Store.ArticleViewingType)o.ArticleViewingType,
                TeaserImageUrl = o.TeaserImageUrl,
                NewsItemIds = o.NewsItemIds,
            };
        }

        public Store.NewsItem Convert(NewsItem o)
        {
            return new Store.NewsItem
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
