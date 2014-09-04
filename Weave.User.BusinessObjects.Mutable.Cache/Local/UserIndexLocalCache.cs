using Common.Caching;
using System;

namespace Weave.User.BusinessObjects.Mutable.Cache.Local
{
    class UserIndexLocalCache
    {
        readonly LRUCache<Guid, UserIndex> cache;

        public UserIndexLocalCache()
        {
            // limited to the latest N users
            cache = new LRUCache<Guid, UserIndex>(2000);
        }

        public UserIndex Get(Guid id)
        {
            return cache[id];
        }

        public void Set(UserIndex user)
        {
            cache[user.Id] = user;
        }
    }
}