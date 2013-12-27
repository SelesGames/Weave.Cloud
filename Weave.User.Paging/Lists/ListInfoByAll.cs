using System.Collections.Generic;
using Weave.User.Paging.News;

namespace Weave.User.Paging.Lists
{
    public class ListInfoByAll : ListInfoBase
    {
        public List<PagedNewsByAll> PagedNews { get; set; }
    }
}
