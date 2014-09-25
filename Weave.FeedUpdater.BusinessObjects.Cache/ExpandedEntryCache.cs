using Common.Caching;
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
        readonly LRUCache<Guid, ExpandedEntry> localCache;
        readonly Weave.User.Service.Redis.Clients.ExpandedEntryCache redisCache;
        readonly ExpandedEntryBlobClient blobClient;

        internal ExpandedEntryCache(
            int localCacheLength,
            string azureUserIndexStorageAccountName,
            string azureUserIndexStorageAccountKey,
            string azureUserIndexContainerName)
        {
            this.localCache = new LRUCache<Guid, ExpandedEntry>(localCacheLength);
            var settings = Settings.StandardConnection;
            var db = settings.GetDatabase(DatabaseNumbers.CANONICAL_NEWSITEMS);
            this.redisCache = new Weave.User.Service.Redis.Clients.ExpandedEntryCache(db);

            this.blobClient = new ExpandedEntryBlobClient(
                storageAccountName: azureUserIndexStorageAccountName,
                storageKey: azureUserIndexStorageAccountKey,
                containerName: azureUserIndexContainerName);
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
            var stillMissing = fullResults.Where(o => o.Entry == null).ToList();

            if (stillMissing.Any())
            {
                sw.Start();
                var fromRedis = await redisCache.Get(stillMissing.Select(o => o.Id));
                meta.FromRedis = sw.Record().Dump();

                var zipped = fromRedis.Results.Zip(stillMissing, (result, tuple) => new { result, tuple.i });
                foreach (var temp in zipped)
                {
                    if (temp.result.HasValue)
                    {
                        var entry = temp.result.Value;

                        // null out the heavy fields
                        // (note that going forward, they should have already been nulled out,
                        // but this is for Entries added prior to this update
                        entry.NullOutHeavyFields();

                        var resultTuple = fullResults[temp.i];
                        resultTuple.Entry = entry;
                        resultTuple.Source = "redis";
                        localCache.AddOrUpdate(entry.Id, entry);
                    }
                }
            }

            // try grabbing from blob storage - if found, add to local cache and redis
            stillMissing = fullResults.Where(o => o.Entry == null).ToList();

            if (stillMissing.Any())
            {
                sw.Start();
                var fromBlob = await Task.WhenAll(stillMissing.Select(o => blobClient.Get(o.Id)));
                meta.FromBlob = sw.Record().Dump();
                var zipped2 = fromBlob.Zip(stillMissing, (result, tuple) => new { result, tuple.i });
                var toSaveToRedis = new List<ExpandedEntry>();
                foreach (var temp in zipped2)
                {
                    if (temp.result.HasValue)
                    {
                        var resultTuple = fullResults[temp.i];
                        var entry = temp.result.Value;

                        // null out heavy fields, which will be set when the Entry is 
                        // grabbed from Azure Blob Storage
                        entry.NullOutHeavyFields();

                        resultTuple.Entry = entry;
                        resultTuple.Source = "blob";
                        localCache.AddOrUpdate(entry.Id, entry);
                        toSaveToRedis.Add(entry);
                    }
                }
                if (toSaveToRedis.Any())
                    await new Redis.SaveCommand().Execute(toSaveToRedis, overWrite: false);
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




        #region helper class

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

        #endregion
    }
}