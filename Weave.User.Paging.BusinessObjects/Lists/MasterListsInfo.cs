using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.Paging.Lists
{
    public class MasterListsInfo
    {
        public Guid UserId { get; set; }
        public List<ListInfoByAll> AllNewsLists { get; set; }
        public List<ListInfoByCategory> CategoryLists { get; set; }
        public List<ListInfoByFeed> FeedLists { get; set; }
    }
}
