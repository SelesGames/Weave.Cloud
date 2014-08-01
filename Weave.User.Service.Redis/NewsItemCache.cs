using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    /// <summary>
    /// The cache for retrieving the actual article/news item content
    /// </summary>
    public class NewsItemCache
    {
        readonly ConnectionMultiplexer connection;
        readonly IDatabase db;
        readonly RedisValueSerializer serializer;

        public NewsItemCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            db = connection.GetDatabase(DatabaseNumbers.INDICES_AND_NEWSCACHE);
            serializer = new RedisValueSerializer(new NewsItemBinarySerializer());
        }

        public async Task<IEnumerable<RedisCacheResult<NewsItem>>> Get(IEnumerable<Guid> newsItemIds)
        {
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
