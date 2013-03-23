using System;
using System.Collections.Generic;

namespace Weave.User.DTOs
{
    public class UserInfo
    {
        public Guid Id { get; set; }
        public int FeedCount { get; set; }
        public List<Feed> Feeds { get; set; }
    }
}