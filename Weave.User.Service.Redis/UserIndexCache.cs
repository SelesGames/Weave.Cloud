using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis.Json;

namespace Weave.User.Service.Redis
{
    public class UserIndexCache
    {
        ConnectionMultiplexer connection;

        public UserIndexCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<RedisCacheResult<UserIndex>> Get(Guid userId)
        {
            var db = connection.GetDatabase(0);
            var key = (RedisKey)userId.ToByteArray();

            var value = await db.StringGetAsync(key, CommandFlags.None);
            var result = value.ReadAs<UserIndex>();
            return result;
        }

        public async Task Save(UserIndex userIndex)
        {
            var db = connection.GetDatabase(0);
            var key = (RedisKey)userIndex.Id.ToByteArray();
            var val = userIndex.WriteAs();

            await db.StringSetAsync(key, val, TimeSpan.FromDays(7), When.Always, CommandFlags.HighPriority);
        }
    }
}
