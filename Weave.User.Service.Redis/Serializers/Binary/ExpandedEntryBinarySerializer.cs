using StackExchange.Redis;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class ExpandedEntryBinarySerializer : RedisValueSerializer<ExpandedEntry>
    {
        protected override ExpandedEntry Map(RedisValue value)
        {
            using (var reader = new ExpandedEntryReader((byte[])value))
            {
                reader.Read();
                return reader.Get();
            }
        }

        protected override RedisValue Map(ExpandedEntry o)
        {
            using (var writer = new ExpandedEntryWriter(o))
            {
                writer.Write();
                return writer.GetBytes();
            }
        }
    }
}