using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    public class CanonicalFeedIndexCache : StandardStringCache<FeedIndex>
    {
        public CanonicalFeedIndexCache(IDatabaseAsync db)
            : base(db, new CanonicalFeedIndexBinarySerializer()) { }

        public Task<RedisCacheResult<FeedIndex>> Get(Guid feedId)
        {
            var key = (RedisKey)feedId.ToByteArray();
            return base.Get(key, CommandFlags.None);
        }

        public async Task<RedisCacheMultiResult<FeedIndex>> Get(IEnumerable<Guid> feedIds)
        {
            var keys = feedIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();

            var result = await base.Get(keys, CommandFlags.None);
            return result;
        }

        public Task<RedisWriteResult<bool>> Save(FeedIndex feedIndex)
        {
            var key = (RedisKey)feedIndex.Id.ToByteArray();

            return base.Set(
                key: key, 
                value: feedIndex, 
                expiry: TimeSpan.FromDays(7), 
                when: When.Always, 
                flags: CommandFlags.HighPriority);
        }
    }
}