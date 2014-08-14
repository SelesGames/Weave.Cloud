using StackExchange.Redis;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class NewsItemBinarySerializer : RedisValueSerializer<NewsItem>
    {
        protected override NewsItem Map(RedisValue value)
        {
            using (var reader = new NewsItemReader((byte[])value))
            {
                reader.Read();
                return reader.GetNewsItem();
            }
        }

        protected override RedisValue Map(NewsItem o)
        {
            using (var writer = new NewsItemWriter(o))
            {
                writer.Write();
                return writer.GetBytes();
            }
        }
    }
}