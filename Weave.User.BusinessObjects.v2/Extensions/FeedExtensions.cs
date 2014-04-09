using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static IEnumerable<Feed> FindByCategory(this IEnumerable<Feed> source, string category)
        {
            if (source == null) throw new ArgumentNullException("feeds");
            if (string.IsNullOrEmpty(category)) throw new Exception("No category specified");

            if ("all news".Equals(category, StringComparison.OrdinalIgnoreCase))
                return source;

            return source.Where(o => category.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Feed> FindById(this IEnumerable<Feed> source, Guid feedId)
        {
            if (source == null) throw new ArgumentNullException("feeds");

            return source.Where(o => feedId == o.Id);
        }

        public static IEnumerable<Feed> FindByIds(this IEnumerable<Feed> source, IEnumerable<Guid> feedIds)
        {
            if (source == null) throw new ArgumentNullException("feeds");
            if (EnumerableEx.IsNullOrEmpty(feedIds)) 
                throw new Exception("No feedIds specified");

            var feeds = from f in source
                        join id in feedIds on f.Id equals id
                        select f;

            if (EnumerableEx.IsNullOrEmpty(feeds))
                throw new Exception(
                    string.Format("No feeds match feedIds: {0}",
                    feedIds.Aggregate(new StringBuilder(), (sb, id) => sb.Append(id + ", ")).ToString()
                    ));

            return feeds;
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