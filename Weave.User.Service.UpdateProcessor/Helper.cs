using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.User.BusinessObjects.Mutable.Cache.Azure;
using Weave.User.BusinessObjects.Mutable.Cache.PubSub;
using Weave.User.Service.Redis;

namespace Weave.User.Service.UpdateProcessor
{
    static class Helper
    {
        public static async Task<IDisposable> StartAsync()
        {
            var clientConnection = Settings.StandardConnection;
            var pubsubConnection = Settings.PubsubConnection;
            
            var redisCache = new UserIndexCache(clientConnection);
            var blobClient = new UserIndexBlobClient(
                storageAccountName: "weaveuser2",
                storageKey: "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                containerName: "userindices");

            var innerHelper = new InnerHelper(redisCache, blobClient);

            var updateNoticeObserver = new UserIndexUpdateEventObserver();
            var disposeHandle = await updateNoticeObserver.Observe(innerHelper.OnUserUpdated);
            return disposeHandle;
        }

        class InnerHelper
        {
            UserIndexCache redisCache;
            UserIndexBlobClient blobClient;

            public InnerHelper(UserIndexCache redisCache, UserIndexBlobClient blobClient)
            {
                this.redisCache = redisCache;
                this.blobClient = blobClient;
            }

            public async void OnUserUpdated(UserIndexUpdateNotice notice)
            {
                if (notice == null)
                    return;

                var userId = notice.UserId;

                var result = await redisCache.Get(userId);

                if (result.HasValue)
                {
                    var user = result.Value;
                    var azureSaveResult = await blobClient.Save(user);
                }
            }
        }
    }
}