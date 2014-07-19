using System;
using System.Collections;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.Mutable
{
    public class NewsItemIndices : IEnumerable<NewsItemIndex>
    {
        List<NewsItemIndex> innerList;
        HashSet<Guid> ids;

        public NewsItemIndices()
        {
            innerList = new List<NewsItemIndex>();
            ids = new HashSet<Guid>();
        }

        /// <summary>
        /// Adds a news item to the index
        /// </summary>
        /// <param name="feed">The newsItem to be added</param>
        /// <returns>True if the newsItem was added, false if the newsItem was already present or invalid</returns>
        public bool TryAdd(NewsItemIndex newsItem)
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
