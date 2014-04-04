using System;

namespace Weave.User.BusinessObjects.v2
{
    public class UserFeedsMediator
    {
        FeedCollection feeds;

        public UserFeedsMediator(UserInfo user)
        {
            this.feeds = user.Feeds;
        }




        #region Add/Remove/Update a feed

        /// <summary>
        /// Adds a feed to the user's collection of feeds
        /// </summary>
        /// <param name="feed">The feed to be added</param>
        /// <param name="trustSource">Will skip checking to see if feed is already present and that Id is set - use for deserialization only</param>
        /// <returns>True if the feed was added, false if the feed was already present or invalid</returns>
        public bool AddFeed(Feed feed, bool trustSource = false)
        {
            if (feed == null) return false;
            if (string.IsNullOrWhiteSpace(feed.Name) || string.IsNullOrWhiteSpace(feed.Uri))
                return false;

            return feeds.TryAdd(feed, trustSource);
        }

        public void RemoveFeed(Guid feedId)
        {
            feeds.RemoveWithId(feedId);
        }

        public void UpdateFeed(Feed feed)
        {
            feeds.Update(feed);
        }

        #endregion
    }
}
