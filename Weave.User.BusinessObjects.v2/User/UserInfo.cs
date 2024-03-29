﻿using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2
{
    public class UserInfo
    {
        public UserInfo()
        {
            Feeds = new FeedCollection();
        }
        
        public Guid Id { get; set; }
        public FeedCollection Feeds { get; private set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }
    }
}
