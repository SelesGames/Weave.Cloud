using StackExchange.Redis;
using System;
using Weave.Updater.Azure;
using Weave.Updater.PubSub;
using Weave.User.Service.Redis;

namespace Weave.Updater.Monitor.Azure.WorkerRole.Startup
{
    internal class StartupTask
    {
        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        readonly TimeSpan pollingInterval = TimeSpan.FromMilliseconds(30);

        readonly ConnectionMultiplexer cm;
        readonly FeedBlobUpdater blobUpdater;
        IDisposable handle;

        public StartupTask()
        {
            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            cm = ConnectionMultiplexer.Connect(redisClientConfig);
            var db = cm.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var redisCache = new Weave.User.Service.Redis.FeedUpdaterCache(db);
            var blobClient = new Weave.Updater.Azure.FeedUpdaterCache(
                "weaveuser2",
                "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                "updaterfeeds");
            blobUpdater = new FeedBlobUpdater(blobClient, redisCache);
        }

        public async void OnStart()
        {
            var bridge = new FeedUpdateEventBridge(cm);
            handle = await bridge.Observe(OnFeedUpdateNotice);
        }

        async void OnFeedUpdateNotice(FeedUpdateNotice notice)
        {
            try
            {
                await blobUpdater.Update(notice.FeedUri);
            }
            catch { }
        }
    }
}