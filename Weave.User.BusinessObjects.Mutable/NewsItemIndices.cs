using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public class NewsItemIndices : IEnumerable<NewsItemIndex>
    {
        readonly FeedIndex feedIndex;
        readonly List<NewsItemIndex> innerList;
        readonly HashSet<Guid> ids;

        public NewsItemIndices(FeedIndex feedIndex)
        {
            this.feedIndex = feedIndex;
            this.innerList = new List<NewsItemIndex>();
            this.ids = new HashSet<Guid>();
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

            if (ids.Add(newsItem.Id))
            {
                innerList.Add(newsItem);
                return true;
            }

            return false;
        }

        public int Total { get { return innerList.Count; } }

        public int CountUnread()
        {
            return innerList.Count(o => !o.HasBeenViewed);
        }

        public int CountNew()
        {
            return innerList.Count(feedIndex.IsNewsItemNew);
        }




        #region IEnumerable interface implementation

        IEnumerator<NewsItemIndex> IEnumerable<NewsItemIndex>.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion
    }
}
