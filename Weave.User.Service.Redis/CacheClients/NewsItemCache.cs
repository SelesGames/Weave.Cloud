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
        readonly IDatabaseAsync db;
        readonly RedisValueSerializer<NewsItem> serializer;

        public NewsItemCache(IDatabaseAsync db)
        {
            this.db = db;
            serializer = new NewsItemBinarySerializer();
        }

        public async Task<IEnumerable<RedisCacheResult<NewsItem>>> Get(IEnumerable<Guid> newsItemIds)
        {
            var keys = newsItemIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();

            var values = await db.StringGetAsync(keys, CommandFlags.None);
            var results = values.Select(serializer.Read);
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
                value = serializer.Write(newsItem);
            }
            catch
            {
                return Task.FromResult(false);
            }

            if (!value.HasValue)
                return Task.FromResult(false);

            return db.StringSetAsync(
                key: key, 
                value: value, 
                expiry: TimeSpan.FromDays(60), 
                when: When.NotExists, 
                flags: CommandFlags.None);
        }
    }
}