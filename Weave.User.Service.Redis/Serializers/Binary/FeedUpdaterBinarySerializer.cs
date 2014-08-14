using StackExchange.Redis;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class FeedUpdaterBinarySerializer : RedisValueSerializer<Feed>
    {
        protected override Feed Map(RedisValue value)
        {
            using (var reader = new FeedUpdaterReader((byte[])value))
            {
                reader.Read();
                return reader.Get();
            }
        }

        protected override RedisValue Map(Feed o)
        {
            using (var writer = new FeedUpdaterWriter(o))
            {
                writer.Write();
                return writer.GetBytes();
            }
        }
    }
}