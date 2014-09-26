using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache;
using Weave.Updater.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.BusinessObjects.Mutable.Cache;

namespace RedisDBHelper
{
    class GetUser
    {
        string userId;
        string feedId;

        public GetUser(string userId)
        {
            this.userId = userId;
        }

        public async Task<string> Execute()
        {
            var cache = UserIndexCacheFactory.CreateCache();
            var user = await cache.Get(Guid.Parse(userId));

            if (user == null)
                return "no matching using found";

            var val = new
            {
                user.Id,
                user.LastModified,
                user.PreviousLoginTime,
                user.CurrentLoginTime,
                ArticleDeletionTimeForMarkedRead = (string)user.ArticleDeletionTimeForMarkedRead,
                ArticleDeletionTimeForUnread= (string)user.ArticleDeletionTimeForUnread,
                TotalNewsCount = user.FeedIndices.AllNewsIndices().Count(),
                TotalFeeds = user.FeedIndices.Count,
            };
            return val.Dump();
        }
    }
}