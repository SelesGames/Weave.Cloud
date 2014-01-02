using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects
{
    public static class FeedExtensions
    {
        //public static IEnumerable<string> UniqueCategoryNames(this IEnumerable<Feed> feeds)
        //{
        //    return feeds.Select(o => o.Category).Distinct().OfType<string>();
        //}

        public static IEnumerable<Feed> OfCategory(this IEnumerable<Feed> feeds, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return feeds;

            return feeds.Where(o => categoryName.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }
    
        public static IEnumerable<NewsItem> AllNews(this IEnumerable<Feed> feeds)
        {
            return feeds.Where(o => o.News != null).SelectMany(o => o.News);
        }

        public static IEnumerable<NewsItem> LatestNewsFirst(this IEnumerable<NewsItem> news)
        {
            return news.OrderByDescending(o => o.UtcPublishDateTime);
        }

        public static IEnumerable<NewsItem> AllOrderedNews(this IEnumerable<Feed> feeds)
        {
            return feeds.AllNews().LatestNewsFirst();
        }
    }
}
