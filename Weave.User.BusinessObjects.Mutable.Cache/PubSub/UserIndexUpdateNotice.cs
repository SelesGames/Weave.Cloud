using System;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    public class UserIndexUpdateNotice
    {
        public Guid UserId { get; set; }
        public Guid CacheId { get; set; }
    }
}