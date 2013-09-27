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

        public UserInfoAzureCacheClient(UserInfoBlobClient userInfoBlobClient)
        {
            this.userInfoBlobClient = userInfoBlobClient;

            // Cache client configured by settings in application configuration file.
            var config = new DataCacheFactoryConfiguration();
            config.SerializationProperties = 
                new DataCacheSerializationProperties(DataCacheObjectSerializerType.CustomSerializer, new UserInfoCacheSerializer());
            var cacheFactory = new DataCacheFactory(config);
            cache = cacheFactory.GetCache(CACHE_NAME);
            writeQueue = new UserInfoBlobWriteQueue(userInfoBlobClient);
        }

        public async Task<UserInfo> Get(Guid userId)
        {
            var key = userId.ToString();

            var o = SafeCacheGet(key);// cache.Get(key);
            if (o != null)
            {
                return o.Cast<UserInfo>();
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
