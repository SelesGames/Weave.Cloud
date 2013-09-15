﻿using Common.Azure;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Threading.Tasks;
using Weave.User.DataStore;
using Weave.User.Service.Cache.Extensions;

namespace Weave.User.Service.Cache
{
    public class AzureDataCacheClient
    {
        readonly string CACHE_NAME = "articlestate";
        DataCache cache;
        IAzureBlobClient blobClient;
        AzureBlobWriteQueue<UserInfo> writeQueue;

        public AzureDataCacheClient(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;

            // Cache client configured by settings in application configuration file.
            var cacheFactory = new DataCacheFactory();
            cache = cacheFactory.GetCache(CACHE_NAME);
            writeQueue = new AzureBlobWriteQueue<UserInfo>(blobClient);
        }

        public async Task<UserInfo> Get(Guid userId)
        {
            var key = userId.ToString();

            var o = cache.Get(key);
            if (o != null)
            {
                return o.Cast<UserInfo>();
            }

            // there was a cache miss if we get this far

            var x = await blobClient.Get<UserInfo>(key);

            cache.Put(key, x);

            return x;
        }

        public void Update(Guid userId, UserInfo user)
        {
            var key = userId.ToString();

            cache.Put(key, user);
            writeQueue.Add(key, user);
        }
    }
}