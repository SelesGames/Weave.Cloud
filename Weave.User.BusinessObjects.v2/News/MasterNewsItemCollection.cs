using System;
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
                var groups = news.GroupBy(o => o.FeedId);
                foreach (var group in groups)
                {
                    Add(group.Key, group);
                }
            }
        }

        public Guid UserId { get; set; }
    }
}
