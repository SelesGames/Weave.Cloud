using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2
{
    /// <summary>
    /// Decorates a Feed with it's News-count-related properties (HasBeenViewed, IsFavorite, etc.)
    /// </summary>
    public class ExtendedFeed
    {
        readonly Feed feed;

        public ExtendedFeed(Feed feed)
        {
            if (feed == null) throw new ArgumentNullException("feed");

            this.feed = feed;
        }

        public Feed Feed { get { return feed; } }

        public IReadOnlyCollection<ExtendedNewsItem> News { get; set; }

        public int NewArticleCount { get; set; }
        public int UnreadArticleCount { get; set; }
        public int TotalArticleCount { get; set; }
    }
}
