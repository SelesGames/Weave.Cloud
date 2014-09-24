﻿using Common.Caching;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache.Azure;
using Weave.Services.Redis.Ambient;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public class ExpandedEntryCache
    {
        const int LOCAL_CACHE_SIZE = 100000;

        readonly LRUCache<Guid, ExpandedEntry> localCache;
        readonly Weave.User.Service.Redis.ExpandedEntryCache redisCache;
        readonly ExpandedEntryBlobClient blobClient;

        internal ExpandedEntryCache(
            string azureUserIndexStorageAccountName,
            string azureUserIndexStorageAccountKey,
            string azureUserIndexContainerName)
        {
            this.localCache = new LRUCache<Guid, ExpandedEntry>(LOCAL_CACHE_SIZE);
            var settings = Settings.StandardConnection;
            var db = settings.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            this.redisCache = new Weave.User.Service.Redis.ExpandedEntryCache(db);

            this.blobClient = new ExpandedEntryBlobClient(
                storageAccountName: azureUserIndexStorageAccountName,
                storageKey: azureUserIndexStorageAccountKey,
                containerName: azureUserIndexContainerName);
        }

        class resultTuple
        {
            public int i { get; set; }
            public Guid Id { get; set; }
            public ExpandedEntry Entry { get; set; }
            public string Source { get; set; }

            public resultTuple(int i, Guid id, ExpandedEntry entry, string source)
            {
                this.i = i;
                this.Id = id;
                this.Entry = entry;
                this.Source = source;
            }
        }

        public async Task<CacheGetMultiResult<ExpandedEntry>> Get(IEnumerable<Guid> ids)
        {
            var sw = new TimingHelper();
            dynamic meta = new ExpandoObject();

            resultTuple[] fullResults;

            sw.Start();
            // try to grab as many from local storage as possible first
            fullResults = ids
                .Select((id, i) => new resultTuple(i, id, localCache.Get(id), "local"))
                .ToArray();
            meta.LocalGet = sw.Record().Dump();

            // for any feeds not found in the local cache, try grabbing from Redis 
            // - if found, add it to local cache
            var notInLocalCache = fullResults.Where(o => o.Entry == null).ToList();

            if (notInLocalCache.Any())
            {
                sw.Start();
                var fromRedis = await redisCache.Get(notInLocalCache.Select(o => o.Id));
                meta.FromRedis = sw.Record().Dump();

                var zipped = fromRedis.Results.Zip(notInLocalCache, (result, tuple) => new { result, tuple.i });
                foreach (var temp in zipped)
                {
                    if (temp.result.HasValue)
                    {
                        var resultTuple = fullResults[temp.i];
                        var entry = temp.result.Value;

                        // null out the description
                        NullOutHeavyFields(entry);

                        resultTuple.Entry = entry;
                        resultTuple.Source = "redis";
                        localCache.AddOrUpdate(entry.Id, entry);
                    }
                }
            }

            // try grabbing from blob storage - if found, add to local cache and redis
            notInLocalCache = fullResults.Where(o => o.Entry == null).ToList();

            if (notInLocalCache.Any())
            {
                sw.Start();
                var fromBlob = await Task.WhenAll(notInLocalCache.Select(o => blobClient.Get(o.Id)));
                meta.FromBlob = sw.Record().Dump();
                var zipped2 = fromBlob.Zip(notInLocalCache, (result, tuple) => new { result, tuple.i });
                var toSaveToRedis = new List<ExpandedEntry>();
                foreach (var temp in zipped2)
                {
                    if (temp.result.HasValue)
                    {
                        var resultTuple = fullResults[temp.i];
                        var entry = temp.result.Value;

                        // null out the description
                        NullOutHeavyFields(entry);

                        resultTuple.Entry = entry;
                        resultTuple.Source = "blob";
                        localCache.AddOrUpdate(entry.Id, entry);
                        toSaveToRedis.Add(entry);
                    }
                }
                if (toSaveToRedis.Any())
                    await redisCache.Set(toSaveToRedis, overwrite: true);
            }

            var results = fullResults.Select(o => 
                new CacheGetResult<ExpandedEntry>
                {
                    Value = o.Entry,
                    Source = o.Source,
                });

                //o.Entry == null ? 
                //Option.None<ExpandedEntry>() 
                //: 
                //Option.Some(o.Entry));

            return new CacheGetMultiResult<ExpandedEntry>
            {
                Results = results,
                Meta = meta,
            };
        }

        static void NullOutHeavyFields(ExpandedEntry entry)
        {
            entry.Description = null;
            entry.OriginalRssXml = null;
        }

        /// <summary>
        /// On saving a user, save to the local cache, save to Redis, 
        /// notify via PubSub that other local caches need to update from Redis.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<CacheSaveResult> Save(IEnumerable<ExpandedEntry> entries, bool overWrite)
        {
            dynamic meta = new ExpandoObject();
            var sw = new TimingHelper();

            var blobSaveStrategy = SelectBlobSaveStrategy(overWrite);

            sw.Start();
            var blobSaveResults = await Task.WhenAll(entries.Select(blobSaveStrategy));
            meta.BlobSave = sw.Record().Dump();

            // after saving to blob storage, null out heavy fields
            foreach (var entry in entries)
                NullOutHeavyFields(entry);

            sw.Start();
            var redisResult = await redisCache.Set(entries, overwrite: overWrite);
            meta.RedisSave = sw.Record().Dump();

            return new CacheSaveResult
            {
                RedisSaves = redisResult.Results.Select(o => o.ResultValue),
                BlobSaves = blobSaveResults,
            };
        }

        Func<ExpandedEntry, Task<bool>> SelectBlobSaveStrategy(bool overWrite)
        {
            if (overWrite)
                return blobClient.Save;

            else
                return blobClient.ConditionalSave;
        }
    }
}