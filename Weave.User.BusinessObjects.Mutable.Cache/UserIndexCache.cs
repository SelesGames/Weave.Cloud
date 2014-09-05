using Common.Caching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.Mutable.Cache.Azure;
using Weave.User.BusinessObjects.Mutable.Cache.Azure.Legacy;

namespace Weave.User.BusinessObjects.Mutable.Cache
{
    public class UserIndexCache
    {
        readonly Guid cacheId;
        readonly LRUCache<Guid, UserIndex> localCache;
        readonly Weave.User.Service.Redis.UserIndexCache redisCache;
        readonly UserIndexBlobClient blobClient;
        readonly UserInfoBlobClient legacyDataStoreBlobClient;

        public UserIndexCache(
            ConnectionMultiplexer cm, 
            string azureUserIndexStorageAccountName,
            string azureUserIndexStorageAccountKey,
            string )
        {
            this.cacheId = Guid.NewGuid();
            this.localCache = new LRUCache<Guid, UserIndex>(2000);
            this.redisCache = new Service.Redis.UserIndexCache(cm);
            this.blobClient = new UserIndexBlobClient(
                storageAccountName: azureUserIndexStorageAccountName,
                storageKey: azureUserIndexStorageAccountKey,
                containerName: "userindices");
            this.legacyDataStoreBlobClient = new UserInfoBlobClient(
                accountName: )
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
                var wasSavedToRedis = await redisCache.Save(user);
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

            localCache.AddOrUpdate(user.Id,  user);
            var wasSavedToRedis = await redisCache.Save(user);
            if (wasSavedToRedis)
            {
                // send notice here
            }

            return true;
        }
    }
}
