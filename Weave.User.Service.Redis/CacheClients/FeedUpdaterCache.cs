using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    /// <summary>
    /// The cache for retrieving the actual article/news item content
    /// </summary>
    public class FeedUpdaterCache : StandardStringCache<Feed>
    {
        public FeedUpdaterCache(IDatabaseAsync db)
            : base(db, new FeedUpdaterBinarySerializer()) { }

        public Task<RedisCacheResult<Feed>> Get(string feedUrl)
        {
            var key = (RedisKey)feedUrl;
            return base.Get(key, CommandFlags.None);
        }

        public Task<RedisCacheMultiResult<Feed>> Get(IEnumerable<string> feedUrls)
        {
            var keys = feedUrls.Select(o => (RedisKey)o).ToArray();
            return base.Get(keys, CommandFlags.None);
        }

        public async Task<RedisWriteResult<bool>> Save(Feed feed)
        {
            var key = (RedisKey)feed.Uri;
            var result = await base.Set(
                key: key,
                value: feed,
                expiry: TimeSpan.FromDays(7),
                when: When.Always,
                flags: CommandFlags.HighPriority);

            return result;
        }
    }
}