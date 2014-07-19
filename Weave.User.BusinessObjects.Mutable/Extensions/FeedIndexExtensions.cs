using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public static class FeedIndexExtensions
    {
        public static IEnumerable<FeedIndex> OfCategory(this IEnumerable<FeedIndex> feeds, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return new List<FeedIndex>();

            if ("all news".Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                return feeds;

            return feeds.Where(o => categoryName.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<NewsItemIndex> Ordered(this IEnumerable<FeedIndex> feeds)
        {
            return feeds
                .Where(o => o.NewsItemIndices != null)
                .SelectMany(o => o.NewsItemIndices)
                .OrderByDescending(o => o.UtcPublishDateTime);
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
    }
}