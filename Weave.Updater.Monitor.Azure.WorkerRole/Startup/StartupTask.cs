using System;
using Weave.Services.Redis.Ambient;
using Weave.Updater.Azure;
using Weave.Updater.PubSub;
using Weave.User.Service.Redis;

namespace Weave.Updater.Monitor.Azure.WorkerRole.Startup
{
    internal class StartupTask
    {
        readonly FeedBlobUpdater blobUpdater;
        IDisposable handle;

        public StartupTask()
        {
            var standardConnection = Settings.StandardConnection;
            var db = standardConnection.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            var redisCache = new Weave.User.Service.Redis.FeedUpdaterCache(db);
            var blobClient = new Weave.Updater.Azure.FeedUpdaterCache(
                "weaveuser2",
                "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                "updaterfeeds");
            blobUpdater = new FeedBlobUpdater(blobClient, redisCache);
        }

        public async void OnStart()
        {
            var pubsubConnection = Settings.PubsubConnection;
            var observer = new FeedUpdateObserver(pubsubConnection);
            handle = await observer.Observe(OnFeedUpdateNotice);
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