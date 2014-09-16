using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
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
            var db = Settings.StandardConnection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var wasDeleted = await db.KeyDeleteAsync(url);
            return wasDeleted ? "deleted" : "no key found";
        }
    }
}