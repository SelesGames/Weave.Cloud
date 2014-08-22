using StackExchange.Redis;
using System;

namespace Weave.User.Service.Redis.Serializers
{
    public abstract class RedisValueSerializer<T>
    {
        protected abstract T Map(RedisValue value);
        protected abstract RedisValue Map(T o);

        public RedisCacheResult<T> Read(RedisValue val)
        {
            if (val.IsNullOrEmpty || !val.HasValue)
                return new RedisCacheResult<T> { RedisValue = val };//, RedisCacheResult.Create(default(T), val);

            byte[] array = (byte[])val;
            try
            {
                var result = Map(array);
                return new RedisCacheResult<T> { RedisValue = val, Value = result };
                //return RedisCacheResult.Create(result, val);
            }
            catch(Exception ex)
            {
                var serializationException = new SerializationException(ex);
                return new RedisCacheResult<T> { RedisValue = val, SerializationException = serializationException };
            }
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