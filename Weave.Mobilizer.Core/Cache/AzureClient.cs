using Common.Azure.SmartBlobClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Weave.Readability;

namespace Weave.Mobilizer.Core.Service
{
    public class AzureClient
    {
        CloudStorageAccount csa;
        string account, key;

        public AzureClient(string account, string key)//StorageCredentialsAccountAndKey accountAndKey)
        {
            this.account = account;
            this.key = key;
            this.csa = new CloudStorageAccount(new StorageCredentialsAccountAndKey(account, key), false);//accountAndKey, false);
        }

        public async Task Save(string url, ReadabilityResult result)
        {
            var client = new SmartBlobClient(account, key, "articles", false)
            {
                ContentType = "application/json; charset=utf-8",
                UseGzipOnUpload = true,
                WriteTimeout = TimeSpan.FromMinutes(3),
            };

            await client.Save(url, result);



            //var blobClient = csa.CreateCloudBlobClient();
            //var blobContainer = blobClient.GetContainerReference("articles");
            ////var permissions = blobContainer.GetPermissions();
            ////permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            ////blobContainer.CreateIfNotExist();

            //var fileName = UrlToFileName(url);

            //var blob = blobContainer.GetBlobReference(fileName);
            //blob.Properties.ContentType = "application/json; charset=utf-8";
            ////blob.Properties.ContentEncoding = "gzip";

            //BlobRequestOptions options = new BlobRequestOptions();
            //options.AccessCondition = AccessCondition.None;
            //options.Timeout = TimeSpan.FromMinutes(3);

            //using (var ms = new MemoryStream())
            //{
            //    var serial = new DataContractJsonSerializer(typeof(ReadabilityResult));
            //    serial.WriteObject(ms, result);
            //    ms.Position = 0;
            //    await blob.UploadFromStreamAsync(ms, options);
            //}
            Debug.WriteLine(string.Format("{0} uploaded to azure", url), "AZURE");
        }

        public Task<ReadabilityResult> Get(string url)
        {
            var client = new SmartBlobClient(account, key, "articles", false)
            {
                ReadTimeout = TimeSpan.FromMinutes(8),
            };

            return client.Get<ReadabilityResult>(url);

            //var blobClient = csa.CreateCloudBlobClient();
            //var blobContainer = blobClient.GetContainerReference("articles");

            //var fileName = UrlToFileName(url);

            //var blob = blobContainer.GetBlobReference(fileName);

            //BlobRequestOptions options = new BlobRequestOptions();
            //options.AccessCondition = AccessCondition.None;
            //options.Timeout = TimeSpan.FromSeconds(8);

            //using (var ms = new MemoryStream())
            //{
            //    await blob.DownloadToStreamAsync(ms, options);
            //    ms.Position = 0;
            //    var serial = new DataContractJsonSerializer(typeof(ReadabilityResult));
            //    var result = (ReadabilityResult)serial.ReadObject(ms);
            //    //Debug.WriteLine(string.Format("{0} retrieved from azure", url), "AZURE");
            //    return result;
            //}
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

        // approach using MD5 and GUIDs
        string UrlToFileName(string url)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(url);
            byte[] hash = md5.ComputeHash(inputBytes);
            Guid guid = new Guid(hash);
            string hashString = guid.ToString("N");
            return hashString;
        }
    }

    public static class CloudExtensions
    {
        public static Task DeleteAsync(this CloudBlob blob)
        {   
            return Task.Factory.FromAsync(blob.BeginDelete, blob.EndDelete, null);
        }

        //public static Task DownloadToStreamAsync(this CloudBlob blob, Stream stream, BlobRequestOptions options)
        //{
        //    return Task.Factory.FromAsync<Stream, BlobRequestOptions>(blob.BeginDownloadToStream, blob.EndDownloadToStream, stream, options, null);
        //}

        //public static Task UploadFromStreamAsync(this CloudBlob blob, Stream stream, BlobRequestOptions options)
        //{
        //    return Task.Factory.FromAsync<Stream, BlobRequestOptions>(blob.BeginUploadFromStream, blob.EndUploadFromStream, stream, options, null);
        //}
    }
}
