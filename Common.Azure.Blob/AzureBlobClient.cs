using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace Common.Azure.Blob
{
    /// <summary>
    /// A thread-safe class for accessing Azure Blob Storage
    /// </summary>
    public class AzureBlobClient
    {
        public CloudStorageAccount account;
        public string BlobEndpoint { get; private set; }




        #region Constructors

        public AzureBlobClient(string storageAccountName, string key, bool useHttps)
        {
            var blobCred = new StorageCredentials(storageAccountName, key);
            account = new CloudStorageAccount(blobCred, useHttps);
            BlobEndpoint = account.BlobEndpoint.ToString();
        }

        public AzureBlobClient(CloudStorageAccount account)
        {
            this.account = account;
            BlobEndpoint = account.BlobEndpoint.ToString();
        }

        #endregion




        #region Get Handles (Reference) to Containers or Blobs

        public CloudBlobContainer GetContainerHandle(string containerName)
        {
            var client = account.CreateCloudBlobClient();
            return client.GetContainerReference(containerName);
        }

        public ICloudBlob GetBlobHandle(
            string containerName, 
            string blobName, 
            DateTimeOffset? snapshotTime = null)
        {
            var container = GetContainerHandle(containerName);
            var blob = container.GetBlockBlobReference(blobName, snapshotTime);
            return blob;
        }

        #endregion
    }
}