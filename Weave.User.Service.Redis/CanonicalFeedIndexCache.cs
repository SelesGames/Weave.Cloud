using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    public class CanonicalFeedIndexCache
    {
        readonly ConnectionMultiplexer connection;
        RedisValueSerializer2<FeedIndex> serializer;

        public CanonicalFeedIndexCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            this.serializer = new CanonicalFeedIndexBinarySerializer();
        }

        public async Task<RedisCacheResult<FeedIndex>> Get(Guid feedId)
        {
            var db = connection.GetDatabase(DatabaseNumbers.INDICES_AND_NEWSCACHE);
            var key = (RedisKey)feedId.ToByteArray();

            var value = await db.StringGetAsync(key, CommandFlags.None);
            var cacheResult = serializer.Read(value);
            return cacheResult;
        }

        public Task<bool> Save(FeedIndex feedIndex)
        {
            var db = connection.GetDatabase(DatabaseNumbers.INDICES_AND_NEWSCACHE);
            var key = (RedisKey)feedIndex.Id.ToByteArray();
            var val = serializer.Write(feedIndex);

            return db.StringSetAsync(
                key: key, 
                value: val, 
                expiry: TimeSpan.FromDays(7), 
                when: When.Always, 
                flags: CommandFlags.HighPriority);
        }
    }
}
