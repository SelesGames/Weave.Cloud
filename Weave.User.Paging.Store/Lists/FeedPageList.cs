using System;
using System.Collections.Generic;

namespace Weave.User.Paging.Store.Lists
{
    public class FeedPageList
    {
        public Guid FeedId { get; set; }
        public Guid? LatestListId { get; set; }
        public DateTime? LatestRefresh { get; set; }
        public List<ListInfo> Lists { get; set; }
    }
}
