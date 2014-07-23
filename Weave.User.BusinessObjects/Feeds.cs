﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects
{
    public class Feeds : IEnumerable<Feed>
    {
        UserInfo user;
        List<Feed> innerList;
        HashSet<Guid> ids;

        public Feeds(UserInfo user)
        {
            this.user = user;
            innerList = new List<Feed>();
            ids = new HashSet<Guid>();
        }

        /// <summary>
        /// Adds a feed to the user's collection of feeds
        /// </summary>
        /// <param name="feed">The feed to be added</param>
        /// <returns>True if the feed was added, false if the feed was already present or invalid</returns>
        public bool Add(Feed feed)
        {
            if (feed == null) return false;
            if (string.IsNullOrWhiteSpace(feed.Name) || string.IsNullOrWhiteSpace(feed.Uri))
                return false;

            feed.EnsureGuidIsSet();

            if (ids.Add(feed.Id))
            {
                innerList.Add(feed);
                feed.User = user;
                return true;
            }

            return false;
        }

        public void Update(Feed feed)
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

        public int Count { get { return innerList.Count; } }




        #region IEnumerable interface implementation

        IEnumerator<Feed> IEnumerable<Feed>.GetEnumerator()
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