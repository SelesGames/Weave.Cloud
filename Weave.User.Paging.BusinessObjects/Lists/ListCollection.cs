using System;
using System.Collections.Generic;

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
    }
}
