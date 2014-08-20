using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    /// <summary>
    /// The cache for retrieving the actual article/news item content
    /// </summary>
    public class NewsItemCache : StandardStringCache<NewsItem>
    {
        public NewsItemCache(IDatabaseAsync db)
            : base(db, new NewsItemBinarySerializer()) { }

        public Task<RedisCacheMultiResult<NewsItem>> Get(IEnumerable<Guid> newsItemIds)
        {
            var keys = newsItemIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();
            return base.Get(keys, CommandFlags.None);
        }

        public async Task<RedisWriteMultiResult<bool>> Set(IEnumerable<NewsItem> newsItems)
        {
            var requests = newsItems.Select(CreateSaveRequest);
            var results = await Task.WhenAll(requests);

            return new RedisWriteMultiResult<bool>
            {
                Results = results,
                Timings = !Timings.AreEnabled ? CacheTimings.Empty :
                    results.Select(o => o.Timings).Aggregate(
                        new CacheTimings(), (accum, result) =>
                        {
                            return new CacheTimings
                            {
                                SerializationTime = accum.SerializationTime + result.SerializationTime,
                                ServiceTime = accum.ServiceTime + result.ServiceTime,
                            };
                        }),
            };
        }

        Task<RedisWriteResult<bool>> CreateSaveRequest(NewsItem newsItem)
        {
            if (newsItem == null)
                return Task.FromResult(new RedisWriteResult<bool> { ResultValue = false, Timings = CacheTimings.Empty });

            var key = (RedisKey)newsItem.Id.ToByteArray();

            return base.Set(
                key: key, 
                value: newsItem, 
                expiry: TimeSpan.FromDays(60), 
                when: When.NotExists, 
                flags: CommandFlags.None);
        }
    }
}