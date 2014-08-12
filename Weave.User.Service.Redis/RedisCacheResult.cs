using StackExchange.Redis;

namespace Weave.User.Service.Redis
{
    public class RedisCacheResult<T>
    {
        public RedisValue RedisValue { get; internal set; }
        public bool HasValue { get { return RedisValue.HasValue && Value is T; } }
        public T Value { get; internal set; }
    }

    static class RedisCacheResult
    {
        public static RedisCacheResult<T> Create<T>(T val, RedisValue redisVal)
        {
            return new RedisCacheResult<T> { Value = val, RedisValue = redisVal };
        }
    }
}
