using System;
using System.Collections.Generic;

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
    }
}
