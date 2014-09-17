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

        public async Task<RedisCacheResult<Feed>> Get(string feedUrl)
        {
            var key = (RedisKey)feedUrl;
            var result = await base.Get(key, CommandFlags.None);
            ApplyUrlFix(result, feedUrl);

            return result;
        }

        public async Task<RedisCacheMultiResult<Feed>> Get(IEnumerable<string> feedUrls)
        {
            var keys = feedUrls.Select(o => (RedisKey)o).ToArray();
            var multiResult = await base.Get(keys, CommandFlags.None);
            var zipped = feedUrls.Zip(multiResult.Results, (url, result) => new { url, result });
            var fixedResults = new List<RedisCacheResult<Feed>>();
            foreach (var tuple in zipped)
            {
                ApplyUrlFix(tuple.result, tuple.url);
                fixedResults.Add(tuple.result);
            }
            multiResult.Results = fixedResults;
            return multiResult;
        }

        public async Task<RedisWriteResult<bool>> Save(Feed feed)
        {
            var key = (RedisKey)feed.Uri;
            var result = await base.Set(
                key: key,
                value: feed,
                expiry: TimeSpan.FromDays(30),
                when: When.Always,
                flags: CommandFlags.HighPriority);

            return result;
        }

        static void ApplyUrlFix(RedisCacheResult<Feed> result, string feedUrl)
        {
            if (result.HasValue)
            {
                result.Value.Uri = feedUrl;
            }
        }
    }
}