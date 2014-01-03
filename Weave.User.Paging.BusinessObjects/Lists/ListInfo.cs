using System;
using System.Collections.Generic;
using Weave.User.Paging.BusinessObjects.News;

namespace Weave.User.Paging.BusinessObjects.Lists
{
    public class ListInfo
    {
        public Guid ListId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccess { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public List<PagedNewsBase> PagedNews { get; set; }
    }
}
