using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.Serializers;

namespace Weave.User.Service.Redis.Clients
{
    public class StandardStringCache<T>
    {
        readonly IDatabaseAsync db;
        readonly RedisValueSerializer<T> serializer;
        readonly TimingHelper sw;

        public StandardStringCache(IDatabaseAsync db, RedisValueSerializer<T> serializer)
        {
            this.db = db;
            this.serializer = serializer;
            this.sw = new TimingHelper();
        }

        protected async Task<RedisCacheResult<T>> Get(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var timings = new CacheTimings();

            sw.Start();
            var value = await db.StringGetAsync(key, flags);
            timings.ServiceTime = sw.Record();

            sw.Start();
            var cacheResult = serializer.Read(value);
            timings.SerializationTime = sw.Record();

            cacheResult.RedisKey = key;
            cacheResult.Timings = timings;
            return cacheResult;
        }

        protected async Task<RedisCacheMultiResult<T>> Get(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var timings = new CacheTimings();

            sw.Start();
            var values = await db.StringGetAsync(keys, CommandFlags.None);
            timings.ServiceTime = sw.Record();

            sw.Start();
            var results = values.Select(serializer.Read);
            timings.SerializationTime = sw.Record();

            foreach (var tuple in keys.Zip(results, (key, result) => new { key, result }))
            {
                tuple.result.RedisKey = tuple.key;
            }

            var cacheResult = new RedisCacheMultiResult<T>
            {
                Results = results,
                Timings = timings,
            };
            return cacheResult;
        }

        protected async Task<RedisWriteResult<bool>> Set(RedisKey key, T value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var timings = new CacheTimings();

            sw.Start();
            var val = serializer.Write(value);
            timings.SerializationTime = sw.Record();

            sw.Start();
            var result = await  db.StringSetAsync(key, val, expiry, when, flags);
            timings.ServiceTime = sw.Record();

            var writeResult = new RedisWriteResult<bool>
            {
                RedisKey = key,
                RedisValue = val,
                ResultValue = result,
                Timings = timings,
            };
            return writeResult;
        }
    }
}