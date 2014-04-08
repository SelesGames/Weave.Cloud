using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public static class ExtendedNewsItemExtensions
    {
        public static IEnumerable<ExtendedNewsItem> LatestNewsFirst(this IEnumerable<ExtendedNewsItem> news)
        {
            return news.OrderByDescending(o => o.NewsItem.UtcPublishDateTime);
        }
    }
}