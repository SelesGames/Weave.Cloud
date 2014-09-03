using Common.Azure.Blob;
using Common.Azure.SmartBlobClient;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.Updater.Azure.Blob
{
    public class FeedUpdaterCache
    {
        readonly SmartBlobClient blobClient;
        readonly string containerName;

        public FeedUpdaterCache(string storageAccountName, string storageKey, string containerName)
        {
            this.blobClient = new SmartBlobClient(storageAccountName, storageKey, false);
            this.containerName = containerName;
        }

        public async Task<Feed> Get(string feedUrl)
        {
            var result = await blobClient.Get<Feed>(containerName, feedUrl);
            return result.Value;
        }

        public async Task<bool> Save(Feed feed)
        {
            var requestProperties = new WriteRequestProperties
            {
                UseCompression = true, 
            };
            var blobName = feed.Uri;
            try
            {
                await blobClient.Save(containerName, blobName, feed, requestProperties);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}