using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects.Mutable.Extensions.Helpers;

namespace Weave.User.BusinessObjects.Mutable
{
    public static class FeedIndexExtensions
    {
        public static IEnumerable<FeedIndex> WithId(this IEnumerable<FeedIndex> feeds, Guid feedId)
        {
            return feeds.Where(o => o.Id == feedId);
        }

        public static IEnumerable<FeedIndex> OfCategory(this IEnumerable<FeedIndex> feeds, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return new List<FeedIndex>();

            if ("all news".Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                return feeds;

            return feeds.Where(o => categoryName.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<NewsItemIndexFeedIndexTuple> AllIndices(this IEnumerable<FeedIndex> feeds)
        {
            return feeds
                .Where(o => o.NewsItemIndices != null)
                .SelectMany(o => o.NewsItemIndices
                    .Select(x => new NewsItemIndexFeedIndexTuple(x, o)));
        }

        static NewsItemIndexFeedIndexTupleComparer orderedComparer = new NewsItemIndexFeedIndexTupleComparer();

        public static IEnumerable<NewsItemIndexFeedIndexTuple> Ordered(this IEnumerable<NewsItemIndexFeedIndexTuple> indices)
        {
            return indices
                .OrderByDescending(o => o.IsNew)
                .ThenByDescending(o => o.NewsItemIndex.UtcPublishDateTime)
                .Distinct(orderedComparer);
        }

        public static void MarkEntry(this IEnumerable<FeedIndex> feeds)
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var now = DateTime.UtcNow;
            foreach (var feed in feeds)
            {
                feed.PreviousEntrance = feed.MostRecentEntrance;
                feed.MostRecentEntrance = now;
            }
        }

        public static void ExtendEntry(this IEnumerable<FeedIndex> feeds)
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var now = DateTime.UtcNow;
            foreach (var feed in feeds)
            {
                feed.MostRecentEntrance = now;
            }
        }

        public static IEnumerable<NewsItemIndexFeedIndexTuple> GetLatestNews(this IEnumerable<FeedIndex> feeds)
        {
            return LatestNewsHelper.GetTopNewsItems(feeds);
        }




        #region Helper class for the Ordered function

        class NewsItemIndexFeedIndexTupleComparer : IEqualityComparer<NewsItemIndexFeedIndexTuple>
        {
            public bool Equals(NewsItemIndexFeedIndexTuple x, NewsItemIndexFeedIndexTuple y)
            {
                if (x == y)
                    return true;

                var xNews = x.NewsItemIndex;
                var yNews = y.NewsItemIndex;

                return
                    (xNews.Id == yNews.Id) ||
                    (xNews.TitleHash == yNews.TitleHash) ||
                    (xNews.UrlHash == yNews.UrlHash);
            }

            public int GetHashCode(NewsItemIndexFeedIndexTuple obj)
            {
                return obj.NewsItemIndex.Id.GetHashCode();
            }
        }

        #endregion
    }
}