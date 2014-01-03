using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects;

namespace Weave.User.Paging.BusinessObjects.Lists
{
    public class ListCollection
    {
        public Guid UserId { get; set; }
        public List<CategoryPageList> CategoryLists { get; set; }
        public List<FeedPageList> FeedLists { get; set; }

        public ListCollection()
        {
            CategoryLists = new List<CategoryPageList>();
            FeedLists = new List<FeedPageList>();
        }

        public DateTime? GetLatestRefreshForFeed(Feed feed)
        {
            var mostRecentList = FeedLists
                .Where(o => o.FeedId == feed.Id)
                .SelectMany(o => o.Lists)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefault();

            return mostRecentList == null ? null : (DateTime?)mostRecentList.CreatedOn;
        }

        public DateTime? GetLatestRefreshForCategory(string category)
        {
            var mostRecentList = CategoryLists
                .Where(o => o.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .SelectMany(o => o.Lists)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefault();

            return mostRecentList == null ? null : (DateTime?)mostRecentList.CreatedOn;
        }
    }
}
