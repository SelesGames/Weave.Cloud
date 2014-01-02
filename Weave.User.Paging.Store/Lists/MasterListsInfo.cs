using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.Paging.Store.Lists
{
    //[DataContract]
    public class MasterListsInfo
    {
        public Guid UserId { get; set; }
        public List<ListInfoByAll> AllNewsLists { get; set; }
        public List<ListInfoByCategory> CategoryLists { get; set; }
        public List<ListInfoByFeed> FeedLists { get; set; }
    }
}
