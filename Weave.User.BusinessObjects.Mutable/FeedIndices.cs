using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public class FeedIndices : IEnumerable<FeedIndex>
    {
        List<FeedIndex> innerList;
        HashSet<Guid> ids;

        public FeedIndices()
        {
            innerList = new List<FeedIndex>();
            ids = new HashSet<Guid>();
        }

        /// <summary>
        /// Adds a feed to the user's collection of feeds
        /// </summary>
        /// <param name="feed">The feed to be added</param>
        /// <param name="trustSource">Will skip checking to see if feed is already present and that Id is set - use for deserialization only</param>
        /// <returns>True if the feed was added, false if the feed was already present or invalid</returns>
        public bool TryAdd(FeedIndex feed, bool trustSource = false)
        {
            if (feed == null) return false;
            if (string.IsNullOrWhiteSpace(feed.Name) || string.IsNullOrWhiteSpace(feed.Uri))
                return false;

            // if we don't trust the Feed was created correctly, verify it's Id
            if (!trustSource)
            {
                feed.EnsureGuidIsSet();
            }

            if (ids.Add(feed.Id))
            {
                innerList.Add(feed);
                return true;
            }

            return false;
        }

        public void Update(FeedIndex feed)
        {
            if (feed == null ||string.IsNullOrWhiteSpace(feed.Name)) 
                return;

            var matching = this.FirstOrDefault(o => o.Id.Equals(feed.Id));
            if (matching != null)
            {
                // the only 3 fields the user can change are category, feed name, and article viewing type
                matching.Category = feed.Category;
                matching.Name = feed.Name;
                matching.ArticleViewingType = feed.ArticleViewingType;
            }
        }

        public void RemoveWithId(Guid feedId)
        {
            var matching = this.FirstOrDefault(o => o.Id.Equals(feedId));
            if (matching != null)
            {
                ids.Remove(feedId);
                innerList.Remove(matching);
            }
        }




        #region IEnumerable interface implementation

        IEnumerator<FeedIndex> IEnumerable<FeedIndex>.GetEnumerator()
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
