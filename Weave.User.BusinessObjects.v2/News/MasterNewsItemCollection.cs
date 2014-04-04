using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public class MasterNewsItemCollection : Dictionary<Guid, IEnumerable<NewsItem>>
    {
        //readonly IDictionary<Guid, IEnumerable<NewsItem>> table;

        public MasterNewsItemCollection(IEnumerable<NewsItem> news)
        {
            if (news == null) throw new ArgumentNullException("news");

            //this.table = news.GroupBy(o => o.Id).ToDictionary(o => o.Key, o => o.AsEnumerable());

            foreach (var group in news.GroupBy(o => o.Id))
            {
                Add(group.Key, group);
            }
        }

        public Guid UserId { get; set; }



        //public List<NewsItem> News { get; set; }
    }
}
