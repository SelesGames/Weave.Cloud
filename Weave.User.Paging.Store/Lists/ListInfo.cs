using System;

namespace Weave.User.Paging.Store.Lists
{
    public class ListInfo
    {
        public Guid ListId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccess { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }
}
