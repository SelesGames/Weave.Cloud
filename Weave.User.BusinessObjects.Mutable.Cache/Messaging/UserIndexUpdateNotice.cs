using System;

namespace Weave.User.BusinessObjects.Mutable.Cache.Messaging
{
    public class UserIndexUpdateNotice
    {
        public Guid UserId { get; set; }
        public Guid CacheId { get; set; }
    }
}