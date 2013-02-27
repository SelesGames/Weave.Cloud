using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Azure
{
    public class AzureBlobStreamClient
    {
        CloudStorageAccount account;
        string container;

        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }
        public string ContentType { get; set; }
        public string BlobEndpoint { get; private set; }

        public AzureBlobStreamClient(string storageAccountName, string key, string container, bool useHttps)
        {
            var blobCred = new StorageCredentialsAccountAndKey(storageAccountName, key);
            account = new CloudStorageAccount(blobCred, useHttps);
            BlobEndpoint = account.BlobEndpoint.ToString();
            this.container = container;

            ReadTimeout = TimeSpan.FromSeconds(30);
            WriteTimeout = TimeSpan.FromMinutes(1);
        }

        public async Task<Stream> Get(string fileName)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(fileName);

            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = ReadTimeout;

            var ms = new MemoryStream();
            await blob.DownloadToStreamAsync(ms, options);
            ms.Position = 0;
            return ms;
        }

        public Task Save(string fileName, Stream stream)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(fileName);

            if (!string.IsNullOrEmpty(ContentType))
                blob.Properties.ContentType = ContentType;


            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = WriteTimeout;

            return blob.UploadFromStreamAsync(stream, options);
        }

        public Task Delete(string fileName)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(fileName);

            return blob.DeleteAsync();
        }
    }
}