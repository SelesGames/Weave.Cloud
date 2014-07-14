using StackExchange.Redis;

namespace Weave.User.Service.Redis
{
    public class RedisCacheResult<T>
    {
        public RedisValue RedisValue { get; set; }
        public T Value { get; set; }
    }

    static class RedisCacheResult
    {
        public static RedisCacheResult<T> Create<T>(T val, RedisValue redisVal)
        {
            return new RedisCacheResult<T> { Value = val, RedisValue = redisVal };
        }
    }
}
