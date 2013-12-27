﻿using System;
using System.Runtime.Serialization;

namespace Weave.User.Paging.Store.Lists
{
    [DataContract]
    public class ListInfoByAll
    {
        public Guid ListId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccess { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }
}
