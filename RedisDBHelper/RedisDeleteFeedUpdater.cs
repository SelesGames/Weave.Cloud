using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Service.Redis;

namespace RedisDBHelper
{
    class RedisDeleteFeedUpdater
    {
        readonly string url;

        public RedisDeleteFeedUpdater(string url)
        {
            this.url = url;
        }

        public async Task<string> Execute()
        {
            var config = new RedisConfig();
            var db = config.CreateConnection().GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var wasDeleted = await db.KeyDeleteAsync(url);
            return wasDeleted ? "deleted" : "no key found";
        }
    }
}