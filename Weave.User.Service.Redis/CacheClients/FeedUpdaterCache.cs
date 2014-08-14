using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    /// <summary>
    /// The cache for retrieving the actual article/news item content
    /// </summary>
    public class FeedUpdaterCache
    {
        readonly IDatabaseAsync db;
        readonly RedisValueSerializer<Feed> serializer;

        public FeedUpdaterCache(IDatabaseAsync db)
        {
            this.db = db;
            serializer = new FeedUpdaterBinarySerializer();
        }

        public async Task<RedisCacheResult<Feed>> Get(Guid feedId)
        {
            var key = (RedisKey)feedId.ToByteArray();

            var value = await db.StringGetAsync(key, CommandFlags.None);
            var result = serializer.Read(value);
            return result;
        }

        public Task<bool> Save(Feed feed)
        {
            var key = (RedisKey)feed.Id.ToByteArray();
            var val = serializer.Write(feed);

            return db.StringSetAsync(
                key: key,
                value: val,
                expiry: TimeSpan.FromDays(7),
                when: When.Always,
                flags: CommandFlags.HighPriority);
        }
    }
}