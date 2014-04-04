using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2
{
    public class UserInfo
    {
        public Guid Id { get; set; }
        public FeedCollection Feeds { get; set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }
    }
}
