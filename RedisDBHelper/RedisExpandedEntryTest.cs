using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var config = new RedisConfig();
            var db = config.CreateConnection().GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
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