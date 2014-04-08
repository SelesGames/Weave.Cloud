using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public static class FeedExtensions
    {
        public static IEnumerable<string> GetFullCategoryList(this IEnumerable<Feed> feeds)
        {
            var categories = new List<string> { "all news" };
            categories.AddRange(feeds.Select(o => o.Category).Distinct().OfType<string>());
            return categories;
        }

        public static IEnumerable<Feed> OfCategory(this IEnumerable<Feed> feeds, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return new List<Feed>();

            if ("all news".Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                return feeds;

            return feeds.Where(o => categoryName.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }

        public static void MarkEntry(this IEnumerable<Feed> feeds)
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

        public static void ExtendEntry(this IEnumerable<Feed> feeds)
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