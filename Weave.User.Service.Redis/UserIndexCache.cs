using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable;
using Weave.User.Service.Redis.Serializers;
using Weave.User.Service.Redis.Serializers.Binary;

namespace Weave.User.Service.Redis
{
    public class UserIndexCache
    {
        readonly ConnectionMultiplexer connection;
        readonly RedisValueSerializer serializer;

        public UserIndexCache(ConnectionMultiplexer connection)
        {
            this.connection = connection;
            serializer = new RedisValueSerializer(new UserIndexBinarySerializer());
        }

        public async Task<RedisCacheResult<UserIndex>> Get(Guid userId)
        {
            var db = connection.GetDatabase(DatabaseNumbers.USER_INDICES);
            var key = (RedisKey)userId.ToByteArray();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var value = await db.StringGetAsync(key, CommandFlags.None);
            sw.Stop();
            DebugEx.WriteLine("the actual getting of the user index took {0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var cacheResult = serializer.ReadAs<UserIndex>(value);
            sw.Stop();
            DebugEx.WriteLine("deserializing the user index took {0} ms", sw.ElapsedMilliseconds);

            return cacheResult;
        }

        public Task<bool> Save(UserIndex userIndex)
        {
            var db = connection.GetDatabase(DatabaseNumbers.USER_INDICES);
            var key = (RedisKey)userIndex.Id.ToByteArray();
            var val = serializer.WriteAs(userIndex);

            return db.StringSetAsync(
                key: key, 
                value: val, 
                expiry: TimeSpan.FromDays(7), 
                when: When.Always, 
                flags: CommandFlags.HighPriority);
        }
    }
}