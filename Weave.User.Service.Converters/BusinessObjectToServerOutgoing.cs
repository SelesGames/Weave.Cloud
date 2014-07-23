using SelesGames.Common;
using System.Linq;
using Weave.User.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Outgoing = Weave.User.Service.DTOs.ServerOutgoing;


namespace Weave.User.Service.Converters
{
    public class BusinessObjectToServerOutgoing :
        IConverter<UserIndex, Outgoing.UserInfo>,
        IConverter<FeedIndex, Outgoing.Feed>,
        IConverter<Image, Outgoing.Image>
    {
        public static readonly BusinessObjectToServerOutgoing Instance = new BusinessObjectToServerOutgoing();

        public Outgoing.Image Convert(Image o)
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

        public Outgoing.NewsItem Convert(NewsItem o)
        {
            return new Outgoing.NewsItem
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
                IsNew = o.IsNew(),
                HasBeenViewed = o.HasBeenViewed,
                IsFavorite = o.IsFavorite,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                Image = o.Image == null ? null : Convert(o.Image),
            };
        }

        public Outgoing.Feed Convert(FeedIndex o)
        {
            return new Outgoing.Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                IconUri = o.IconUri,
                Category = o.Category,
                ArticleViewingType = (Weave.User.Service.DTOs.ArticleViewingType)o.ArticleViewingType,
                TotalArticleCount = o.NewsItemIndices.Count,
                NewArticleCount = o.NewsItemIndices.CountNew(),
                UnreadArticleCount = o.NewsItemIndices.CountUnread(),
                TeaserImageUrl = o.TeaserImageUrl,
                LastRefreshedOn = o.LastRefreshedOn,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };
        }

        public Outgoing.UserInfo Convert(UserIndex o)
        {
            return new Outgoing.UserInfo
            {
                Id = o.Id,
                FeedCount = o.FeedIndices.Count,
                Feeds = o.FeedIndices.Select(Convert).ToList(),
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread,
            };
        }
    }
}
