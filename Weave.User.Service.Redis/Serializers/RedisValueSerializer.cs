using StackExchange.Redis;

namespace Weave.User.Service.Redis.Serializers
{
    class RedisValueSerializer
    {
        IByteSerializer innerSerializer;

        public RedisValueSerializer(IByteSerializer innerSerializer)
        {
            this.innerSerializer = innerSerializer;
        }

        public RedisCacheResult<T> ReadAs<T>(RedisValue val)
        {
            if (val.IsNullOrEmpty || !val.HasValue)
                return RedisCacheResult.Create<T>(default(T), val);

            byte[] array = (byte[])val;
            var result = innerSerializer.ReadObject<T>(array);

            return RedisCacheResult.Create(result, val);
        }

        public RedisValue WriteAs<T>(T o)
        {
            try
            {
                var bytes = innerSerializer.WriteObject(o);
                return (RedisValue)bytes;
            }
            catch(System.Exception ex)
            {
                DebugEx.WriteLine(ex);
                throw;
            }
        }
    }
}