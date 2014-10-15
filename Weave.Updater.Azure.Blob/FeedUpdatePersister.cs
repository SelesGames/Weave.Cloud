using System;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Clients;
using Weave.User.Service.Redis.Communication.Generic;

namespace Weave.FeedUpdater.Messaging
{
    public class FeedUpdatePersister : RedisPersister<FeedUpdateNotice, Feed>
    {
        public FeedUpdatePersister(string account, string key, string container)
            : base(
            new FeedUpdateMessageQueue(), 
            CreateGet(), 
            CreatePersist(account, key, container),
            TimeSpan.FromMilliseconds(500))
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
            var blobClient = new Azure.FeedUpdaterCache(account, key, container);
            return val => blobClient.Save(val);
        }
    }
}