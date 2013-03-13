using System;
using System.Collections.Generic;

namespace Weave.AccountManagement
{
    public class UserInfo
    {
        public Guid Id { get; set; }
        public int FeedCount { get; set; }
        public List<Feed> Feeds { get; set; }
    }
}
