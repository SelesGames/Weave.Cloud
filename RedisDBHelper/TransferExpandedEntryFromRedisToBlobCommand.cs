using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;

namespace RedisDBHelper
{
    public class TransferExpandedEntryFromRedisToBlobCommand
    {
        public async Task<string> Execute()
        {
            var connectionMultiplexer = Settings.ElevatedConnection;
            var db = connectionMultiplexer.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var redisCache = new Weave.User.Service.Redis.Clients.ExpandedEntryCache(db);
            var saveHelper = ExpandedEntryCacheFactory.CreateSaveHelper();

            var server = connectionMultiplexer.GetServer("weaveuser.redis.cache.windows.net", 6379);
            var allKeys = server.Keys(DatabaseNumbers.CANONICAL_NEWSITEMS, pageSize: 100);

            foreach (var key in allKeys)
            {
                var id = new Guid((byte[])key);
                var val = await redisCache.Get(new[] { id });

                var result = val.Results.Where(o => o.HasValue).Select(o => o.Value).FirstOrDefault();

                if (result != null)
                {
                    await saveHelper.Save(new[] { result }, overWrite: true);
                }
            }

            return "done";
        }
    }
}