using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Threading.Tasks;

namespace Common.Azure
{
    public class AzureBlobClient
    {
        CloudStorageAccount account;

        public string BlobEndpoint { get; private set; }

        public AzureBlobClient(string storageAccountName, string key, bool useHttps)
        {
            var blobCred = new StorageCredentialsAccountAndKey(storageAccountName, key);
            account = new CloudStorageAccount(blobCred, useHttps);
            BlobEndpoint = account.BlobEndpoint.ToString();
        }

        protected CloudBlobContainer GetContainerHandle(string containerName)
        {
            var client = account.CreateCloudBlobClient();
            return client.GetContainerReference(containerName);
        }

        public Task<bool> CreateContainer(string containerName, BlobRequestOptions options = null)
        {
            var container = GetContainerHandle(containerName);
            return container.CreateIfNotExistAsync(options);
        }

        public Task DeleteContainer(string containerName, BlobRequestOptions options = null)
        {
            var container = GetContainerHandle(containerName);
            return container.DeleteAsync(options);
        }
    }
}
