using SelesGames.Common;
using System.Linq;
using Weave.UserFeedAggregator.BusinessObjects;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;


namespace Weave.UserFeedAggregator.Converters
{
    public class BusinessObjectToServerOutgoing :
        IConverter<UserInfo, Outgoing.UserInfo>,
        IConverter<Feed, Outgoing.Feed>,
        IConverter<NewsItem, Outgoing.NewsItem>,
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

        public Outgoing.Feed Convert(Feed o)
        {
            return new Outgoing.Feed
            {
                Id = o.Id,
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (Weave.UserFeedAggregator.DTOs.ArticleViewingType)o.ArticleViewingType,
                TotalArticleCount = o.News == null ? 0 : o.News.Count,
                NewArticleCount = o.News == null ? 0 : o.News.Count(x => x.IsNew()),
                UnreadArticleCount = o.News == null ? 0 : o.News.Count(x => !x.HasBeenViewed),
                TeaserImageUrl = o.TeaserImageUrl,
                LastRefreshedOn = o.LastRefreshedOn,
                MostRecentEntrance = o.MostRecentEntrance,
                PreviousEntrance = o.PreviousEntrance,
            };
        }

        public Outgoing.UserInfo Convert(UserInfo o)
        {
            return new Outgoing.UserInfo
            {
                Id = o.Id,
                FeedCount = o.Feeds == null ? 0 : o.Feeds.Count,
                Feeds = o.Feeds == null ? null : o.Feeds.OfType<Feed>().Select(Convert).ToList(),
                PreviousLoginTime = o.PreviousLoginTime,
                CurrentLoginTime = o.CurrentLoginTime,
            };
        }
    }
}
