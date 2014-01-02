using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects;

namespace Weave.User.Paging.Lists
{
    public class MasterListsInfo
    {
        public Guid UserId { get; set; }
        public List<ListInfoByAll> AllNewsLists { get; set; }
        public List<ListInfoByCategory> CategoryLists { get; set; }
        public List<ListInfoByFeed> FeedLists { get; set; }

        public MasterListsInfo()
        {
            AllNewsLists = new List<ListInfoByAll>();
            CategoryLists = new List<ListInfoByCategory>();
            FeedLists = new List<ListInfoByFeed>();
        }

        public DateTime? GetLatestRefreshForFeed(Feed feed)
        {
            var mostRecentList = FeedLists
                .Where(o => o.FeedId == feed.Id)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefault();

            return mostRecentList == null ? null : (DateTime?)mostRecentList.CreatedOn;
        }

        public DateTime? GetLatestRefreshForCategory(string category)
        {
            var mostRecentList = CategoryLists
                .Where(o => o.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefault();

            return mostRecentList == null ? null : (DateTime?)mostRecentList.CreatedOn;
        }
    }
}
