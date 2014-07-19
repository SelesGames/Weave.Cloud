using System;
using System.Collections.Generic;

namespace Weave.User.Service.Redis.DTOs
{
    class UserIndex
    {
        public Guid Id { get; set; }
        public List<FeedIndex> FeedIndices { get; set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }
    }
}