using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.Paging.BusinessObjects.Lists
{
    public abstract class BasePageList
    {
        List<ListInfo> lists;

        public Guid? LatestListId { get; set; }
        public DateTime? LatestRefresh { get; set; }

        public IReadOnlyList<ListInfo> Lists
        {
            get { return lists; }
            set { lists = value == null ? null : value.ToList(); }
        }

        public abstract string CreateFileName();

        public void Add(ListInfo list)
        {
            if (lists == null)
                lists = new List<ListInfo>();
            lists.Add(list);

            if (!lists.Any(o => o.CreatedOn > list.CreatedOn))
            {
                LatestListId = list.ListId;
                LatestRefresh = list.CreatedOn;
            }
        }
    }
}
