using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.Service.Redis
{
    public class RedisCacheResult<T>
    {
        public RedisKey RedisKey { get; internal set; }
        public RedisValue RedisValue { get; internal set; }

        public bool HasValue { get { return RedisValue.HasValue && Value is T; } }
        public T Value { get; internal set; }
        public int ByteLength { get { return GetByteLength(RedisValue); } }

        public CacheTimings Timings { get; internal set; }

        static int GetByteLength(RedisValue value)
        {
            if (value.HasValue && !value.IsInteger)
                return ((byte[])value).Length;
            else
                return -1;
        }
    }

    public class RedisCacheMultiResult<T>
    {
        public IEnumerable<RedisCacheResult<T>> Results { get; internal set; }
        public CacheTimings Timings { get; internal set; }

        public IEnumerable<T> GetValidValues()
        {
            return Results == null ? new List<T>() : Results.Where(o => o.HasValue).Select(o => o.Value);
        }
    }

    public class RedisWriteResult<T>
    {
        public RedisKey RedisKey { get; internal set; }
        public RedisValue RedisValue { get; internal set; }

        public T ResultValue { get; internal set; }

        public CacheTimings Timings { get; internal set; }
    }

    public class RedisWriteMultiResult<T>
    {
        public IEnumerable<RedisWriteResult<T>> Results { get; internal set; }
        public CacheTimings Timings { get; internal set; }
    }

    public class CacheTimings
    {
        public TimeSpan ServiceTime { get; set; }
        public TimeSpan SerializationTime { get; set; }

        public static CacheTimings Empty = new CacheTimings { SerializationTime = TimeSpan.Zero, ServiceTime = TimeSpan.Zero };
    }

    static class RedisCacheResult
    {
        public static RedisCacheResult<T> Create<T>(T val, RedisValue redisVal)
        {
            return new RedisCacheResult<T> { Value = val, RedisValue = redisVal };
        }
    }
}