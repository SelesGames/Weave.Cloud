using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.Mutable
{
    public class UserIndex
    {        
        public Guid Id { get; set; }
        public FeedIndices FeedIndices { get; private set; }
        public Articles Articles { get; private set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }

        public UserIndex()
        {
            FeedIndices = new FeedIndices();
            Articles = new Articles(this);
        }
    }
}
