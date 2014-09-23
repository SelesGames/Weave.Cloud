using System;
using System.Collections.Generic;

namespace Weave.FeedUpdater.BusinessObjects.Cache.Azure.Serialization
{
    public class ExpandedEntry
    {
        public Guid Id { get; set; }
        public List<FeedIndex> FeedIndices { get; set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }

        public DateTime LastModified { get; set; }
    }

    public class FeedIndex
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public string Category { get; set; }
        public string TeaserImageUrl { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }

        // "New" determination and bookkeeping
        public DateTime MostRecentEntrance { get; set; }
        public DateTime PreviousEntrance { get; set; }

        public List<NewsItemIndex> NewsItemIndices { get; set; }
    }

    public class NewsItemIndex
    {
        public Guid Id { get; set; }
        public DateTime UtcPublishDateTime { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }
        public bool HasImage { get; set; }
    }
}