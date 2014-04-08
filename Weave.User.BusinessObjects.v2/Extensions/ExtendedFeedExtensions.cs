using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public static class ExtendedFeedExtensions
    {
        public static IEnumerable<ExtendedNewsItem> AllNews(this IEnumerable<ExtendedFeed> feeds)
        {
            return feeds.Where(o => o.News != null).SelectMany(o => o.News);
        }

        public static IEnumerable<ExtendedNewsItem> AllOrderedNews(this IEnumerable<ExtendedFeed> feeds)
        {
            return feeds.AllNews().LatestNewsFirst();
        }
    }
}