using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable.Extensions.Helpers
{
    static class LatestNewsHelper
    {
        const int pageSize = 8;

        public static IEnumerable<NewsItemIndex> GetTopNewsItems(IEnumerable<FeedIndex> feeds)
        {
            IEnumerable<NewsItemIndex> pool = feeds
                .Ordered()
                .Where(news => !news.HasBeenViewed)
                //.Distinct(NewsItemComparer.Instance)
                .Take(20)
                .ToList();

            pool = CreatePool(pool);
            var topNewsItems = pool.Take(pageSize).ToList();
            return topNewsItems;
        }

        static IEnumerable<NewsItemIndex> CreatePool(IEnumerable<NewsItemIndex> allNewsItems)
        {
            return allNewsItems
                .Select(i => new
                {
                    NewsItem = i,
                    AdjustedSortRating = GetAdjustedForImagePresenceSortRating(i),
                })
                .Select(i =>
                new
                {
                    NewsItem = i.NewsItem,
                    FinalAdjustedSortRating = GetAdjustedForRepetitiveFeedSortRating(i.AdjustedSortRating)
                })
                .OrderByDescending(i => i.FinalAdjustedSortRating)
                .Select(i => i.NewsItem);
        }

        static double GetAdjustedForImagePresenceSortRating(NewsItemIndex i)
        {
            var sortRating = CalculateSortRating(i.UtcPublishDateTime);
            return i.HasImage ? 100d * sortRating : sortRating;
        }

        static double GetAdjustedForRepetitiveFeedSortRating(double i)
        {
            return i;
        }

        static double CalculateSortRating(DateTime dateTime)
        {
            double elapsedHours = (DateTime.UtcNow - dateTime).TotalHours;
            if (elapsedHours <= 0)
                elapsedHours = 0.0001;
            double value = 1d / elapsedHours;
            return value;
        }
    }
}
