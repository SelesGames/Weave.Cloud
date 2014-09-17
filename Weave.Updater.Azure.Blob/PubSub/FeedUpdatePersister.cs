using System;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.PubSub;

namespace Weave.Updater.PubSub
{
    public class FeedUpdatePersister : RedisPersister<FeedUpdateNotice, Feed>
    {
        public FeedUpdatePersister(string account, string key, string container)
            : base(new FeedUpdateObserver(), CreateGet(), CreatePersist(account, key, container))
        { }

        static Func<FeedUpdateNotice,Task<RedisCacheResult<Feed>>> CreateGet()
        {
            var connection = Settings.StandardConnection;
            var db = connection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
 	        var redisCache = new FeedUpdaterCache(db);
            return update => redisCache.Get(update.FeedUri);
        }

        static Func<Feed, Task<bool>> CreatePersist(string account, string key, string container)
        {
            var blobClient = new Weave.Updater.Azure.FeedUpdaterCache(account, key, container);
            return val => blobClient.Save(val);
        }
    }
}