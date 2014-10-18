using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public static class NewsItemIndexFeedIndexTupleExtensions
    {
        static comparerImpl comparer = new comparerImpl();

        public static IEnumerable<NewsItemIndexFeedIndexTuple> AllIndices(this IEnumerable<FeedIndex> feeds, UserIndex userIndex, bool useNormalDetermination)
        {
            var now = DateTime.UtcNow;
            var markedReadCutoffDate = Subtract(now, userIndex.ArticleDeletionTimeForMarkedRead);
            var unreadCutoffDate = Subtract(now, userIndex.ArticleDeletionTimeForUnread);

            return feeds
                .SelectMany(feedIndex => feedIndex
                    .NewsItemIndices
                    .AsParallel()
                    .Select(o => new NewsItemIndexFeedIndexTuple(o, feedIndex, markedReadCutoffDate, unreadCutoffDate, useNormalDetermination)));
        }

        static DateTime Subtract(DateTime val, TimeSpan offset)
        {
            try
            {
                return val - offset;
            }
            catch { }
            return DateTime.MinValue;
        }

        public static IEnumerable<NewsItemIndexFeedIndexTuple> Ordered(this IEnumerable<NewsItemIndexFeedIndexTuple> indices)
        {
            //var array = indices.ToArray();
            //Array.Sort<NewsItemIndexFeedIndexTuple>(array, comparer);
            //return array;

            //return indices
            //    .OrderByDescending(o => o.isNew)
            //    .ThenByDescending(o => o.utcPublishDateTime)
            //    //.Distinct(orderedComparer);
            //    .Distinct();
            var helper = new OrderedNewsItemSetHelper(indices);
            return helper.set;
        }

        class OrderedNewsItemSetHelper
        {
            static comparerImpl comparer = new comparerImpl();

            internal SortedSet<NewsItemIndexFeedIndexTuple> set;

            public OrderedNewsItemSetHelper(IEnumerable<NewsItemIndexFeedIndexTuple> source)
            {
                set = new SortedSet<NewsItemIndexFeedIndexTuple>(source, comparer);
            }
        }

        class comparerImpl : IComparer<NewsItemIndexFeedIndexTuple>
        {
            public int Compare(NewsItemIndexFeedIndexTuple x, NewsItemIndexFeedIndexTuple y)
            {
                // compare by id
                if (x.Id == y.Id)
                    return 0;

                // compare by "isnew"
                if (x.IsNew && !y.IsNew)
                    return -1;
                if (!x.IsNew && y.IsNew)
                    return 1;

                // items are either both new or both not new - compare by publish time
                if (x.UtcPublishDateTime <= y.UtcPublishDateTime)
                    return 1;
                else
                    return -1;
            }
        }




        //public static IEnumerable<NewsItemIndexFeedIndexTuple> ThatAreNotDeleted(this IEnumerable<NewsItemIndexFeedIndexTuple> source, UserIndex userIndex)
        //{
        //    var now = DateTime.UtcNow;
        //    var markedReadCutoffDate = Subtract(now, userIndex.ArticleDeletionTimeForMarkedRead);
        //    var unreadCutoffDate = Subtract(now, userIndex.ArticleDeletionTimeForUnread);

        //    return source.Where(o => CanKeep(o, markedReadCutoffDate, unreadCutoffDate));
        //}

        //static bool CanKeep(NewsItemIndexFeedIndexTuple o, DateTime markedReadCutoffDate, DateTime unreadCutoffDate)
        //{
        //    if (o.isNew)
        //        return true;

        //    if (o.isFavorite)
        //        return true;

        //    if (o.hasBeenViewed && o.originalDownloadDateTime > markedReadCutoffDate)
        //        return true;

        //    if (!o.hasBeenViewed && o.originalDownloadDateTime > unreadCutoffDate)
        //        return true;

        //    return false;
        //}

        //static NewsItemIndexFeedIndexTupleComparer orderedComparer = new NewsItemIndexFeedIndexTupleComparer();


        //#region Helper class for the Ordered function

        //class NewsItemIndexFeedIndexTupleComparer : IEqualityComparer<NewsItemIndexFeedIndexTuple>
        //{
        //    public bool Equals(NewsItemIndexFeedIndexTuple x, NewsItemIndexFeedIndexTuple y)
        //    {
        //        return x.NewsItemIndex.Id == y.NewsItemIndex.Id;

        //        //if (x == y)
        //        //    return true;

        //        //var xNews = x.NewsItemIndex;
        //        //var yNews = y.NewsItemIndex;

        //        //return
        //        //    (xNews.Id == yNews.Id); /*||
        //        //    (xNews.TitleHash == yNews.TitleHash) ||
        //        //    (xNews.UrlHash == yNews.UrlHash);*/
        //    }

        //    public int GetHashCode(NewsItemIndexFeedIndexTuple obj)
        //    {
        //        return obj.NewsItemIndex.Id.GetHashCode();
        //    }
        //}

        //#endregion
    }
}
