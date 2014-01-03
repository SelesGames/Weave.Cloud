using System;
using System.Collections.Generic;
using Weave.User.Paging.BusinessObjects.News;

namespace Weave.User.Paging.Lists
{
    public class ListInfoByFeed : ListInfoBase
    {
        public Guid FeedId { get; set; }
        public List<PagedNewsByFeed> PagedNews { get; set; }
    }
}
