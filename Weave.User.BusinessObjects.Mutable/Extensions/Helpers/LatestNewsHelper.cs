using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable.Extensions.Helpers
{
    static class LatestNewsHelper
    {
        const int pageSize = 20;

        public static IEnumerable<NewsItemIndexFeedIndexTuple> GetTopNewsItems(IEnumerable<FeedIndex> feeds)
        {
            IEnumerable<NewsItemIndexFeedIndexTuple> pool = feeds
                .AllIndices()
                .Ordered()
                .Where(o => !o.NewsItemIndex.HasBeenViewed)
                .Take(20)
                .ToList();

            pool = CreatePool(pool);
            var topNewsItems = pool.Take(pageSize).ToList();
            return topNewsItems;
        }

        static IEnumerable<NewsItemIndexFeedIndexTuple> CreatePool(IEnumerable<NewsItemIndexFeedIndexTuple> allNewsItems)
        {
            return allNewsItems
                .Select(i => new
                {
                    NewsItem = i,
                    AdjustedSortRating = GetAdjustedForImagePresenceSortRating(i),
                })
                .OrderByDescending(i => i.AdjustedSortRating)
                .Select(i => i.NewsItem);
        }

        static double GetAdjustedForImagePresenceSortRating(NewsItemIndexFeedIndexTuple i)
        {
            var sortRating = CalculateSortRating(i.NewsItemIndex.UtcPublishDateTime);
            return i.NewsItemIndex.HasImage ? 100d * sortRating : sortRating;
        }

        //static double GetAdjustedForRepetitiveFeedSortRating(double i)
        //{
        //    return i;
        //}

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