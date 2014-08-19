using StackExchange.Redis;

namespace Weave.User.Service.Redis.Serializers
{
    public abstract class RedisValueSerializer<T>
    {
        protected abstract T Map(RedisValue value);
        protected abstract RedisValue Map(T o);

        public RedisCacheResult<T> Read(RedisValue val)
        {
            if (val.IsNullOrEmpty || !val.HasValue)
                return RedisCacheResult.Create(default(T), val);

            byte[] array = (byte[])val;
            var result = Map(array);

            return RedisCacheResult.Create(result, val);
        }

        public RedisValue Write(T o)
        {
            try
            {
                var bytes = Map(o);
                return (RedisValue)bytes;
            }
            catch (System.Exception ex)
            {
                DebugEx.WriteLine(ex);
                throw;
            }
        }
    }
}