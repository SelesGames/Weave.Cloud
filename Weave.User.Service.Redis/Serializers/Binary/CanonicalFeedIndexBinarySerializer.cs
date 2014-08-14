using StackExchange.Redis;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class CanonicalFeedIndexBinarySerializer : RedisValueSerializer<FeedIndex>
    {
        protected override FeedIndex Map(RedisValue value)
        {
            using (var reader = new CanonicalFeedIndexReader((byte[])value))
            {
                reader.Read();
                return reader.GetCanonicalFeedIndex();
            }
        }

        protected override RedisValue Map(FeedIndex o)
        {
            using (var writer = new CanonicalFeedIndexWriter(o))
            {
                writer.Write();
                return writer.GetBytes();
            }
        }
    }
}
