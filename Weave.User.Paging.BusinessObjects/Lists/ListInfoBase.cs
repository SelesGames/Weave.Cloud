using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.Paging.Lists
{
    public abstract class ListInfoBase
    {
        public Guid ListId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccess { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }
}
