﻿using Common.Azure.Compression;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.IO;
using System.Linq;
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
        public bool UseGzipOnUpload { get; set; }

        public AzureBlobStreamClient(string storageAccountName, string key, string container, bool useHttps)
        {
            var blobCred = new StorageCredentialsAccountAndKey(storageAccountName, key);
            account = new CloudStorageAccount(blobCred, useHttps);
            BlobEndpoint = account.BlobEndpoint.ToString();
            this.container = container;

            ReadTimeout = TimeSpan.FromSeconds(30);
            WriteTimeout = TimeSpan.FromMinutes(1);
        }

        public async Task<BlobContent> Get(string fileName)
        {
            var blob = GetBlobHandle(fileName);

            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = ReadTimeout;

            var ms = new MemoryStream();
            await blob.DownloadToStreamAsync(ms, options);
            ms.Position = 0;

            //if ("gzip".Equals(blob.Properties.ContentEncoding, StringComparison.OrdinalIgnoreCase))
            //{
                byte[] byteArray = ms.ToArray();

                var firstTwo = BitConverter.ToString(byteArray.Take(2).ToArray());
                if (firstTwo.Equals("1F-8B"))
                {

                    ms.Dispose();
                    var streamContent = await byteArray.DecompressToStream();
                    return BlobContent.Create(blob, streamContent);
                }
            //}
            return BlobContent.Create(blob, ms);
        }

        public async Task Save(string fileName, Stream stream)
        {
            var blob = GetBlobHandle(fileName);

            if (!string.IsNullOrEmpty(ContentType))
                blob.Properties.ContentType = ContentType;

            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = WriteTimeout;

            if (UseGzipOnUpload)
            {
                blob.Properties.ContentEncoding = "gzip";

                var compressedArray = await stream.CompressToByteArray();
                using (var compressedStream = new MemoryStream(compressedArray))
                {
                    await blob.UploadFromStreamAsync(compressedStream, options);
                }
            }
            else
            {
                await blob.UploadFromStreamAsync(stream, options);
            }
        }

        public Task Delete(string fileName)
        {
            return GetBlobHandle(fileName).DeleteAsync();
        }




        #region Helper methods

        CloudBlob GetBlobHandle(string fileName)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            return container.GetBlobReference(fileName);
        }

        #endregion
    }
}