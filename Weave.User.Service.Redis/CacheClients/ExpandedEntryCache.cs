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
    public class ExpandedEntryCache : StandardStringCache<ExpandedEntry>
    {
        public ExpandedEntryCache(IDatabaseAsync db)
            : base(db, new ExpandedEntryBinarySerializer()) { }

        public Task<RedisCacheMultiResult<ExpandedEntry>> Get(IEnumerable<Guid> entryIds)
        {
            var keys = entryIds.Select(o => (RedisKey)o.ToByteArray()).ToArray();
            return base.Get(keys, CommandFlags.None);
        }

        public async Task<RedisWriteMultiResult<bool>> Set(IEnumerable<ExpandedEntry> entries)
        {
            var requests = entries.Select(CreateSaveRequest);
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

        Task<RedisWriteResult<bool>> CreateSaveRequest(ExpandedEntry entry)
        {
            if (entry == null)
                return Task.FromResult(new RedisWriteResult<bool> { ResultValue = false, Timings = CacheTimings.Empty });

            var key = (RedisKey)entry.Id.ToByteArray();

            return base.Set(
                key: key,
                value: entry, 
                expiry: TimeSpan.FromDays(60), 
                when: When.NotExists, 
                flags: CommandFlags.None);
        }
    }
}