using StackExchange.Redis;
using System;

namespace Weave.User.Service.Redis.Serializers
{
    public interface IRedisValueMap<T>
    {
        T Map(RedisValue val);
        RedisValue Map(T obj);
    }

    public abstract class RedisValueMap<T> : IRedisValueMap<T>
    {
        protected abstract T MapImpl(RedisValue val);
        protected abstract RedisValue MapImpl(T obj);

        public T Map(RedisValue val)
        {
            try
            {
                var mapped = MapImpl(val);
                return mapped;
            }
            catch(Exception ex)
            {
                throw new SerializationException(ex, val);
            }
        }
        
        public RedisValue Map(T obj)
        {
            try
            {
                var mapped = MapImpl(obj);
                return mapped;
            }
            catch (Exception ex)
            {
                throw new SerializationException(ex, RedisValue.Null);
            }
        }
    }
}