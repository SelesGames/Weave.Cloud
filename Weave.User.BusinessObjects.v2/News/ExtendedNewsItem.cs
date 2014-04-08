using System;

namespace Weave.User.BusinessObjects.v2
{
    /// <summary>
    /// Decorates a NewsItem with it's stateful properties (HasBeenViewed, IsFavorite, etc.)
    /// </summary>
    public class ExtendedNewsItem
    {
        readonly NewsItem newsItem;

        public ExtendedNewsItem(NewsItem newsItem)
        {
            if (newsItem == null) throw new ArgumentNullException("newsItem");

            this.newsItem = newsItem;
        }

        public NewsItem NewsItem { get { return newsItem; } }

        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }

        public bool IsNew()
        {
            return !HasBeenViewed && newsItem.OriginalDownloadDateTime > newsItem.Feed.PreviousEntrance;
        }

        public bool IsCountedAsNew()
        {
            return !HasBeenViewed && newsItem.OriginalDownloadDateTime > newsItem.Feed.MostRecentEntrance;
        }
    }
}
