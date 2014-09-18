using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public class NewsItemIndices : IEnumerable<NewsItemIndex>
    {
        const int NUMBER_OF_NEWSITEMS_TO_HOLD = 200;
        static readonly NewsItemIndexComparer indexComparer = new NewsItemIndexComparer();

        readonly FeedIndex feedIndex;
        SortedSet<NewsItemIndex> set;

        public NewsItemIndices(FeedIndex feedIndex)
        {
            this.feedIndex = feedIndex;
            this.set = new SortedSet<NewsItemIndex>(indexComparer);
        }

        /// <summary>
        /// Adds a news item to the index
        /// </summary>
        /// <param name="feed">The newsItem to be added</param>
        /// <returns>True if the newsItem was added, false if the newsItem was already present or invalid</returns>
        public bool Add(NewsItemIndex newsItem)
        {
            if (newsItem == null) return false;
            if (newsItem.Id == Guid.Empty) return false;

            var addResult = set.Add(newsItem);
            if (addResult)
                newsItem.FeedIndex = feedIndex;
            return addResult;
        }

        public bool Remove(NewsItemIndex newsItem)
        {
            if (newsItem == null) return false;
            if (newsItem.Id == Guid.Empty) return false;

            if (set.Remove(newsItem))
                return true;
            else
                return set.RemoveWhere(o => o.Id == newsItem.Id) > 0;
        }

        public int Count { get { return set.Count; } }

        public int CountUnread()
        {
            return set.Count(o => !o.HasBeenViewed);
        }

        public int CountNew()
        {
            return set.Count(feedIndex.IsNewsItemCountedNew);
        }

        public void Trim()
        {
            set = new SortedSet<NewsItemIndex>(set.Take(NUMBER_OF_NEWSITEMS_TO_HOLD), indexComparer);
        }




        #region helper class for doing the EntryWithPostProcessInfo comparisons

        class NewsItemIndexComparer : IComparer<NewsItemIndex>
        {
            public int Compare(NewsItemIndex x, NewsItemIndex y)
            {
                if (x.Id == y.Id)
                    return 0;

                if (x.UtcPublishDateTime <= y.UtcPublishDateTime)
                    return 1;
                else
                    return -1;
            }
        }

        #endregion




        #region IEnumerable interface implementation

        IEnumerator<NewsItemIndex> IEnumerable<NewsItemIndex>.GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return set.GetEnumerator();
        }

        #endregion
    }
}
