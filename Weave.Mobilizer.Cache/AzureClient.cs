using Common.Azure.SmartBlobClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Weave.Readability;

namespace Weave.Mobilizer.Cache
{
    public class AzureClient
    {
        readonly bool USE_HTTPS = false;

        CloudStorageAccount csa;
        string account, key;

        public AzureClient(string account, string key)
        {
            this.account = account;
            this.key = key;
            this.csa = new CloudStorageAccount(new StorageCredentialsAccountAndKey(account, key), USE_HTTPS);
        }

        public async Task Save(string url, ReadabilityResult result)
        {
            var client = new SmartBlobClient(account, key, "articles", USE_HTTPS)
            {
                ContentType = "application/json; charset=utf-8",
                UseGzipOnUpload = true,
                WriteTimeout = TimeSpan.FromMinutes(3),
            };

            await client.Save(url, result);

            Debug.WriteLine(string.Format("{0} uploaded to azure", url), "AZURE");
        }

        public Task<ReadabilityResult> Get(string url)
        {
            var client = new SmartBlobClient(account, key, "articles", USE_HTTPS)
            {
                ReadTimeout = TimeSpan.FromMinutes(8),
            };

            return client.Get<ReadabilityResult>(url);
        }

        public async Task DeleteOlderThan(TimeSpan ttl)
        {
            var now = DateTime.UtcNow;

            var blobClient = csa.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("articles");

            var blobs = blobContainer.ListBlobs().OfType<CloudBlockBlob>();
            foreach (var blob in blobs)
            {
                var lastAccess = blob.Attributes.Properties.LastModifiedUtc;
                var elapsed = now - lastAccess;
                if (elapsed > ttl)
                {
                    await blob.DeleteAsync();
                }
            }
        }
    }

    public static class CloudExtensions
    {
        public static Task DeleteAsync(this CloudBlob blob)
        {   
            return Task.Factory.FromAsync(blob.BeginDelete, blob.EndDelete, null);
        }
    }
}
