using StackExchange.Redis;

namespace Weave.User.Service.Redis
{
    abstract class RedisValueMap<T>
    {
        protected abstract T Map(RedisValue value);
        protected abstract RedisValue Map(T o);
    }
}
