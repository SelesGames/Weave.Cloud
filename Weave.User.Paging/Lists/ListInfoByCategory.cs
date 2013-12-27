using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Paging.News;

namespace Weave.User.Paging.Lists
{
    public class ListInfoByCategory : ListInfoBase
    {
        public string Category { get; set; }
        public List<PagedNewsByCategory> PagedNews { get; set; }
    }
}
