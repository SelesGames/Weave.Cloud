using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Weave.User.Service.Redis
{
    public class RedisCacheResult<T>
    {
        public RedisKey RedisKey { get; internal set; }
        public RedisValue RedisValue { get; internal set; }

        public bool HasValue { get { return RedisValue.HasValue && Value is T; } }
        public T Value { get; internal set; }

        public CacheTimings Timings { get; internal set; }
    }

    public class RedisCacheMultiResult<T>
    {
        public IEnumerable<RedisCacheResult<T>> Results { get; internal set; }
        public CacheTimings Timings { get; internal set; }
    }

    public class RedisWriteResult<T>
    {
        public RedisKey RedisKey { get; internal set; }
        public RedisValue RedisValue { get; internal set; }

        public T ResultValue { get; internal set; }

        public CacheTimings Timings { get; internal set; }
    }

    public class CacheTimings
    {
        public TimeSpan ServiceTime { get; set; }
        public TimeSpan SerializationTime { get; set; }
    }

    static class RedisCacheResult
    {
        public static RedisCacheResult<T> Create<T>(T val, RedisValue redisVal)
        {
            return new RedisCacheResult<T> { Value = val, RedisValue = redisVal };
        }
    }
}
