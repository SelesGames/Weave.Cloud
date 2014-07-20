using StackExchange.Redis;

namespace Weave.User.Service.Redis.Json
{
    static class RedisValueToJsonExtensions
    {
        public static RedisCacheResult<T> ReadAs<T>(this RedisValue val)
        {
            if (val.IsNullOrEmpty || !val.HasValue)
                return RedisCacheResult.Create<T>(default(T), val);

            byte[] array = (byte[])val;
            var serializer = new JsonSerializerHelper();
            var result = serializer.ReadObject<T>(array);

            return RedisCacheResult.Create(result, val);
        }

        public static RedisValue WriteAs<T>(this T o)
        {
            var serializer = new JsonSerializerHelper();
            var bytes = serializer.WriteObject(o);
            return (RedisValue)bytes;
        }
    }
}
