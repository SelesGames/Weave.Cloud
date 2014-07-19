
namespace Weave.User.BusinessObjects.Mutable.Extensions
{
    public static class CopyExtensions
    {
        public static void CopyTo(this UserIndex o, UserInfo user)
        {
            user.Id = o.Id;
            user.PreviousLoginTime = o.PreviousLoginTime;
            user.CurrentLoginTime = o.CurrentLoginTime;
            user.ArticleDeletionTimeForMarkedRead = o.ArticleDeletionTimeForMarkedRead;
            user.ArticleDeletionTimeForUnread = o.ArticleDeletionTimeForUnread;
        }

        public static void CopyTo(this FeedIndex o, Feed feed)
        {
            feed.Id = o.Id;
            feed.Uri = o.Uri;
            feed.Name = o.Name;
            feed.IconUri = o.IconUri;
            feed.Category = o.Category;
            feed.TeaserImageUrl = o.TeaserImageUrl;
            feed.ArticleViewingType = o.ArticleViewingType;
            feed.LastRefreshedOn = o.LastRefreshedOn;
            feed.Etag = o.Etag;
            feed.LastModified = o.LastModified;
            feed.MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate;
            feed.MostRecentEntrance = o.MostRecentEntrance;
            feed.PreviousEntrance = o.PreviousEntrance;
        }

        public static void CopyTo(this NewsItemIndex o, NewsItem newsItem)
        {
            newsItem.Id = o.Id;
            newsItem.IsFavorite = o.IsFavorite;
            newsItem.HasBeenViewed = o.HasBeenViewed;
            newsItem.OriginalDownloadDateTime = o.OriginalDownloadDateTime;
        }
    }
}
