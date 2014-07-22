using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Json;

namespace Weave.User.Service.Redis
{
    public class NewsItemCache
    {
        readonly ConnectionMultiplexer connection;
        readonly IDatabase db;
        readonly RedisValueSerializer serializer;

        public NewsItemCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            db = connection.GetDatabase(0);
            serializer = new RedisValueSerializer(new JsonSerializerHelper());
        }

        public async Task<IEnumerable<RedisCacheResult<NewsItem>>> Get(IEnumerable<Guid> newsItemIds)
        {
            var db = connection.GetDatabase(0);
            var keys = newsItemIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();

            var values = await db.StringGetAsync(keys, CommandFlags.None);
            var results = values.Select(serializer.ReadAs<NewsItem>);
            return results;
        }

        public async Task<IEnumerable<bool>> Set(IEnumerable<NewsItem> newsItems)
        {
            var requests = newsItems.Select(CreateSaveRequest);
            var results = await Task.WhenAll(requests);
            return results;
        }

        Task<bool> CreateSaveRequest(NewsItem newsItem)
        {
            if (newsItem == null)
                return Task.FromResult(false);

            RedisKey key;
            RedisValue value;

            try
            {
                key = (RedisKey)newsItem.Id.ToByteArray();
                value = serializer.WriteAs(newsItem);
            }
            catch
            {
                return Task.FromResult(false);
            }

            if (!value.HasValue)
                return Task.FromResult(false);

            return db.StringSetAsync(key, value, TimeSpan.FromDays(60), When.NotExists, CommandFlags.None);
        }
    }
}
