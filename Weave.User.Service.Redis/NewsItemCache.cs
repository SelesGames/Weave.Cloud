using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;
using Weave.User.Service.Redis.Json;

namespace Weave.User.Service.Redis
{
    public class NewsItemCache
    {
        ConnectionMultiplexer connection;

        public NewsItemCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<IEnumerable<RedisCacheResult<NewsItem>>> Get(IEnumerable<Guid> newsItemIds)
        {
            var db = connection.GetDatabase(0);
            var keys = newsItemIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();

            var values = await db.StringGetAsync(keys, CommandFlags.None);
            var results = values.Select(ReadNewsItem);
            return results;
        }

        RedisCacheResult<NewsItem> ReadNewsItem(RedisValue val)
        {
            if (val.IsNullOrEmpty || !val.HasValue)
                return RedisCacheResult.Create<NewsItem>(null, val);

            byte[] array = (byte[])val;
            var serializer = new JsonSerializerHelper();
            var newsItem = serializer.ReadObject<NewsItem>(array);

            return RedisCacheResult.Create(newsItem, val);
        }
    }
}
