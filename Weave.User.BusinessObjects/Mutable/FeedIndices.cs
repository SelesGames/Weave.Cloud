using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public class FeedIndices : List<FeedIndex>
    {
        /// <summary>
        /// Adds a feed to the user's collection of feeds
        /// </summary>
        /// <param name="feed">The feed to be added</param>
        /// <param name="trustSource">Will skip checking to see if feed is already present and that Id is set - use for deserialization only</param>
        /// <returns>True if the feed was added, false if the feed was already present or invalid</returns>
        public bool TryAdd(Feed feed, bool trustSource = false)
        {
            if (feed == null) return false;
            if (string.IsNullOrWhiteSpace(feed.Name) || string.IsNullOrWhiteSpace(feed.Uri))
                return false;

            // if we don't trust the Feed was created correctly, verify it's Id and that no existing Feed matches
            if (!trustSource)
            {
                feed.EnsureGuidIsSet();

                // if any existing feed has a matching Id, don't add it
                if (this.Any(o => o.Id.Equals(feed.Id)))
                    return false;
            }

            //base.Add(feed);
            return true;
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
                base.Remove(matching);
            }
        }
    }
}
