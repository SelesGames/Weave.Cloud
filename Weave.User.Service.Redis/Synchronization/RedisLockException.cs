using StackExchange.Redis;
using System;

namespace Weave.User.Service.Redis.Synchronization
{
    public class RedisLockException : Exception
    {
        readonly RedisKey key;

        public RedisKey Key { get { return key; } }

        public RedisLockException(RedisKey key)
        {
            this.key = key;
        }
    }
}
