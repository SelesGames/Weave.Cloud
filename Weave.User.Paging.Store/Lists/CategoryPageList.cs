using System;
using System.Collections.Generic;

namespace Weave.User.Paging.Store.Lists
{
    public class CategoryPageList
    {
        public string Category { get; set; }
        public Guid? LatestListId { get; set; }
        public List<ListInfo> Lists { get; set; }
    }
}
