using Common.Azure;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;
using Weave.User.Service.Cache.Extensions;

namespace Weave.User.Service.Cache.nowork
{
    public class AzureDataCacheClient
    {
        readonly string CACHE_NAME = "articlestate";
        DataCache cache;
        IAzureBlobClient blobClient;
        AzureBlobWriteQueue<UserNewsItemState> writeQueue;

        public AzureDataCacheClient(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;

            // Cache client configured by settings in application configuration file.
            var cacheFactory = new DataCacheFactory();
            cache = cacheFactory.GetCache(CACHE_NAME);
            writeQueue = new AzureBlobWriteQueue<UserNewsItemState>(blobClient);
        }

        public async Task<UserNewsItemState> Get(Guid userId)
        {
            var key = userId.ToString();

            var o = cache.Get(key);
            if (o != null)
            {
                return o.Cast<UserNewsItemState>();
            }

            // there was a cache miss if we get this far

            var x = await blobClient.Get<UserNewsItemState>(key);

            cache.Put(key, x);

            return x;
        }

        public void Update(Guid userId, UserNewsItemState state)
        {
            var key = userId.ToString();

            cache.Put(key, state);
            writeQueue.Add(key, state);
        }
    }
}
