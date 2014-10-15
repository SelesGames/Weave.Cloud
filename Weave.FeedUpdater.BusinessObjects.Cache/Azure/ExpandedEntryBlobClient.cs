using Common.Azure.Blob;
using Common.Azure.SmartBlobClient;
using System;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.FeedUpdater.BusinessObjects.Cache.Azure
{
    public class ExpandedEntryBlobClient
    {
        readonly SmartBlobClient blobClient;
        readonly string containerName;

        public ExpandedEntryBlobClient(string storageAccountName, string storageKey, string containerName)
        {
            this.blobClient = new SmartBlobClient(storageAccountName, storageKey, false);
            this.containerName = containerName;
        }

        public async Task<BlobResult<ExpandedEntry>> Get(Guid entryId)
        {
            var blobName = entryId.ToString("N");
            var result = await blobClient.Get<ExpandedEntry>(containerName, blobName);
            return result;
        }

        public async Task<bool> Save(ExpandedEntry entry)
        {
            var requestProperties = new WriteRequestProperties
            {
                UseCompression = true,
            };
            var blobName = entry.Id.ToString("N");
            try
            {
                await blobClient.Save(containerName, blobName, entry, requestProperties);
                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> ConditionalSave(ExpandedEntry entry)
        {
            try
            {
                var blobName = entry.Id.ToString("N");

                var existsAlready = await blobClient.CheckExists(containerName, blobName);
                if (existsAlready)
                    return false;

                var requestProperties = new WriteRequestProperties
                {
                    UseCompression = true,
                };

                await blobClient.Save(containerName, blobName, entry, requestProperties);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }
    }
}