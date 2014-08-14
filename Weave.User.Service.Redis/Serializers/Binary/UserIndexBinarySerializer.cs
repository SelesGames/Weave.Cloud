using StackExchange.Redis;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class UserIndexBinarySerializer : RedisValueSerializer<UserIndex>
    {
        protected override UserIndex Map(RedisValue value)
        {
            using (var reader = new UserIndexReader((byte[])value))
            {
                reader.Read();
                return reader.GetUserIndex();
            }
        }

        protected override RedisValue Map(UserIndex o)
        {
            using (var writer = new UserIndexWriter(o))
            {
                writer.Write();
                return writer.GetBytes();
            }
        }
    }
}