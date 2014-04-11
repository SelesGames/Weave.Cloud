﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public class MasterNewsItemCollection : Dictionary<Guid, IEnumerable<NewsItem>>
    {
        public MasterNewsItemCollection() { }

        public MasterNewsItemCollection(IEnumerable<NewsItem> news)
        {
            if (news != null)
            {
                foreach (var group in news.GroupBy(o => o.Id))
                {
                    Add(group.Key, group);
                }
            }
        }

        public Guid UserId { get; set; }
    }
}
