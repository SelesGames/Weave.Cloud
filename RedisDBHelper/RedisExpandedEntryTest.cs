using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;

namespace RedisDBHelper
{
    class RedisExpandedEntryTest
    {
        readonly string id;

        public RedisExpandedEntryTest(string id)
        {
            this.id = id;
        }

        public async Task<string> Execute()
        {
            var db = Settings.StandardConnection.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            var redisCache = new Weave.User.Service.Redis.ExpandedEntryCache(db);
            var guid = Guid.Parse(id);
            var result = await redisCache.Get(new[] { guid });
            var val = result.Results.FirstOrDefault();
            if (val != null)
                return val.Dump();
            else
                return "not found";
        }
    }
}