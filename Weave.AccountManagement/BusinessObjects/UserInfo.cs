using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.AccountManagement
{
    public class UserInfo
    {
        List<Feed> feeds;

        public Guid Id { get; set; }
        public IReadOnlyList<Feed> Feeds { get; private set; }

        public bool TryAddFeed(Feed feed)
        {
            var id = feed.Id;
            if (feeds.Any(o => id.Equals(o.Id)))
                return false;

            feeds.Add(feed);
            return true;
        }
    }
}
