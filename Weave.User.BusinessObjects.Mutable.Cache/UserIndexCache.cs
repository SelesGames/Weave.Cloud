using Common.Caching;
using System;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.User.BusinessObjects.Mutable.Cache.Azure;
using Weave.User.BusinessObjects.Mutable.Cache.Azure.Legacy;
using Weave.User.BusinessObjects.Mutable.Cache.Messaging;
using Weave.User.Service.Redis.Communication.Generic;

namespace Weave.User.BusinessObjects.Mutable.Cache
{
    public class UserIndexCache : IDisposable
    {
        static readonly TimeSpan POLLINTERVAL = TimeSpan.FromMilliseconds(50);

        readonly Guid cacheId;
        readonly LRUCache<Guid, UserIndex> localCache;
        readonly Weave.User.Service.Redis.Clients.UserIndexCache redisCache;
        readonly UserIndexBlobClient blobClient;
        readonly UserInfoBlobClient legacyDataStoreBlobClient;
        readonly UserIndexUpdateMessageQueue userUpdateMessageQueue;

        IDisposable disposeHandle;

        internal UserIndexCache(
            string azureUserIndexStorageAccountName,
            string azureUserIndexStorageAccountKey,
            string azureUserIndexContainerName,
            string legacyUserDataStoreAccountName,
            string legacyUserDataStoreAccountKey,
            string legacyUserDataStoreContainerName)
        {
            this.cacheId = Guid.NewGuid();
            this.localCache = new LRUCache<Guid, UserIndex>(2000);
            this.redisCache = new Service.Redis.Clients.UserIndexCache(Settings.StandardConnection);

            this.blobClient = new UserIndexBlobClient(
                storageAccountName: azureUserIndexStorageAccountName,
                storageKey: azureUserIndexStorageAccountKey,
                containerName: azureUserIndexContainerName);

            this.legacyDataStoreBlobClient = new UserInfoBlobClient(
                accountName: legacyUserDataStoreAccountName,
                accountKey: legacyUserDataStoreAccountKey,
                containerName: legacyUserDataStoreContainerName);

            this.userUpdateMessageQueue = new UserIndexUpdateMessageQueue();
        }

        public async Task<UserIndex> Get(Guid id)
        {
            UserIndex user;

            // try to grab from local storage first - if successful, return
            user = localCache.Get(id);
            if (user != null)
                return user;

            // try grabbing from Redis - if found, add it to local cache and return
            var fromRedis = await redisCache.Get(id);
            if (fromRedis.HasValue)
            {
                user = fromRedis.Value;
                localCache.AddOrUpdate(id, user);
                return user;
            }

            // try grabbing from blob storage - if found, add to local cache and redis
            var fromBlob = await blobClient.Get(id);
            if (fromBlob.HasValue)
            {
                user = fromBlob.Value;
                localCache.AddOrUpdate(id, user);
                await SaveToRedis(user);
                return user;
            }

            // as a last resort try grabbing from the legacy User Data Store
            user = await legacyDataStoreBlobClient.Get(id);
            if (user != null)
            {
                localCache.AddOrUpdate(id, user);
                await SaveToRedis(user);
                return user;
            }

            return null;
        }

        /// <summary>
        /// On saving a user, save to the local cache, save to Redis, 
        /// notify via PubSub that other local caches need to update from Redis.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> Save(UserIndex user)
        {
            if (user == null)
                throw new ArgumentNullException("user in UserIndexCache.Save");

            user.LastModified = DateTime.UtcNow;

            localCache.AddOrUpdate(user.Id,  user);
            await SaveToRedis(user);

            return true;
        }

        // Begin listening to update notifications
        internal void Initialize()
        {
            disposeHandle = userUpdateMessageQueue.Observe(POLLINTERVAL, OnUpdateNoticeReceived);
        }



        #region helper functions

        // Any time we save to Redis, send a notification that the other local caches 
        // (across other server instances) need to refresh their copy of this user
        async Task SaveToRedis(UserIndex user)
        {
            var wasSavedToRedis = await redisCache.Save(user);
            if (wasSavedToRedis)
            {
                // send notice here
                var notice = new UserIndexUpdateNotice { UserId = user.Id, CacheId = cacheId };
                await userUpdateMessageQueue.Push(notice);
            }
        }

        async Task OnUpdateNoticeReceived(UserIndexUpdateNotice notice)
        {
            // ignore notifications sent from itself
            if (notice.CacheId == cacheId)
                return;

            try
            {
                var userId = notice.UserId;
                var fromRedis = await redisCache.Get(userId);
                if (fromRedis.HasValue)
                {
                    var user = fromRedis.Value;
                    localCache.AddOrUpdate(userId, user);
                }
            }
            catch { }
        }

        #endregion




        public void Dispose()
        {
            if (disposeHandle != null)
                disposeHandle.Dispose();
        }
    }
}