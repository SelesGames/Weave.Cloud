using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Weave.FeedUpdater.BusinessObjects.Cache.Azure;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public class ExpandedEntrySaveHelper
    {
        readonly ExpandedEntryBlobClient blobClient;

        internal ExpandedEntrySaveHelper(
            string azureUserIndexStorageAccountName,
            string azureUserIndexStorageAccountKey,
            string azureUserIndexContainerName)
        {
            this.blobClient = new ExpandedEntryBlobClient(
                storageAccountName: azureUserIndexStorageAccountName,
                storageKey: azureUserIndexStorageAccountKey,
                containerName: azureUserIndexContainerName);
        }

        /// <summary>
        /// Saves ExpanedEntries to both Redis and Azure.  Does not save to the local
        /// LRU cache, since save operations never happen in the User Controller where
        /// they would be useful.
        /// </summary>
        public async Task<CacheSaveResult> Save(IEnumerable<ExpandedEntry> entries, bool overWrite)
        {
            dynamic meta = new ExpandoObject();
            var sw = new TimingHelper();

            var blobSaveStrategy = SelectBlobSaveStrategy(overWrite);

            sw.Start();
            var blobSaveResults = await Task.WhenAll(entries.Select(blobSaveStrategy));
            meta.BlobSave = sw.Record().Dump();

            // after saving to blob storage, null out heavy fields before saving to Redis
            foreach (var entry in entries)
                entry.NullOutHeavyFields();

            sw.Start();
            var redisResult = await new Redis.SaveCommand().Execute(entries, overWrite: overWrite);
            meta.RedisSave = sw.Record().Dump();

            return new CacheSaveResult
            {
                Meta = meta,
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