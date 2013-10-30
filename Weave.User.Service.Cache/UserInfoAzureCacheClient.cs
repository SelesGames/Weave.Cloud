//using Common.Azure.Caching.Local;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;
using Weave.User.Service.Cache.Extensions;

namespace Weave.User.Service.Cache
{
    public class UserInfoAzureCacheClient
    {
        readonly string CACHE_NAME = "user";
        DataCache cache;
        UserInfoBlobWriteQueue writeQueue;
        UserInfoBlobClient userInfoBlobClient;
        //AzureLocalCache cache;

        public UserInfoAzureCacheClient(UserInfoBlobClient userInfoBlobClient)
        {
            this.userInfoBlobClient = userInfoBlobClient;

            // Cache client configured by settings in application configuration file.
            var config = new DataCacheFactoryConfiguration();
            config.SerializationProperties = 
                new DataCacheSerializationProperties(DataCacheObjectSerializerType.CustomSerializer, new UserInfoCacheSerializer());
            var cacheFactory = new DataCacheFactory(config);
            cache = cacheFactory.GetCache(CACHE_NAME);
            //this.cache = new AzureLocalCache(cache);

            //cache.AddCacheLevelBulkCallback(null);
            //cache.AddCacheLevelCallback(
            //    DataCacheOperations.AddItem | DataCacheOperations.RemoveItem | DataCacheOperations.ReplaceItem,
            //    OnCacheUpdated);

            writeQueue = new UserInfoBlobWriteQueue(userInfoBlobClient);
        }

        //void OnCacheUpdated(string cacheName, string regionName, string key, DataCacheItemVersion version, DataCacheOperations cacheOperation, DataCacheNotificationDescriptor nd)
        //{

        //}

        public async Task<UserInfo> Get(Guid userId)
        {
            var key = userId.ToString();

            object user;

            user = SafeCacheGet(key);
            if (user != null && user is UserInfo)
            {
                return (UserInfo)user;
            }

            // there was a cache miss if we get this far

            var x = await userInfoBlobClient.Get(userId);

            cache.Put(key, x);

            return x;
        }

        public void Update(Guid userId, UserInfo user)
        {
            var key = userId.ToString();

            cache.Put(key, user);
            writeQueue.Add(user);
        }

        object SafeCacheGet(string key)
        {
            object result = null;

            try
            {
                result = cache.Get(key);
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            return result;
        }
    }
}
