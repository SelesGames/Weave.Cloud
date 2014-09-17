using StackExchange.Redis;
using System;

namespace Weave.User.Service.Redis.Serializers
{
    public class SerializationException : Exception
    {
        public RedisValue RedisValue { get; private set; }
        public SerializationException(Exception innerException, RedisValue redisValue)
            : base(null, innerException)
        {
            this.RedisValue = redisValue;
        }
    }
}