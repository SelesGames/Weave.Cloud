using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.FeedUpdater.BusinessObjects.Cache.Redis
{
    class SaveCommand
    {
        public Task<RedisWriteMultiResult<bool>> Execute(IEnumerable<ExpandedEntry> entries, bool overWrite)
        {
            var conn = Settings.StandardConnection;
            var db = conn.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var batch = db.CreateBatch();
            var client = new Weave.User.Service.Redis.Clients.ExpandedEntryCache(batch);
            var task = client.Set(entries, overWrite);
            batch.Execute();
            return task;
        }
    }
}