using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Common.Azure.Compression;

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

        public async Task<Stream> Get(string fileName)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(fileName);

            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = ReadTimeout;
            //options.BlobListingDetails

            var ms = new MemoryStream();
            await blob.DownloadToStreamAsync(ms, options);
            ms.Position = 0;

            if ("gzip".Equals(blob.Properties.ContentEncoding, StringComparison.OrdinalIgnoreCase))
            {
                byte[] byteArray = ms.ToArray();

                ms.Dispose();
                ms = new MemoryStream();

                using (var compressStream = new MemoryStream(byteArray))
                using (var decompressor = new GZipStream(compressStream, CompressionMode.Decompress))
                    decompressor.CopyTo(ms);

                ms.Position = 0;
            }
            return ms;
        }

        public async Task Save(string fileName, Stream stream)
        {
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(this.container);
            var blob = container.GetBlobReference(fileName);

            if (!string.IsNullOrEmpty(ContentType))
                blob.Properties.ContentType = ContentType;


            BlobRequestOptions options = new BlobRequestOptions();
            options.AccessCondition = AccessCondition.None;
            options.Timeout = WriteTimeout;

            if (UseGzipOnUpload)
            {
                blob.Properties.ContentEncoding = "gzip";

                using (var compressedStream = await stream.Compress())
                    await blob.UploadFromStreamAsync(compressedStream, options);


                //byte[] byteArray;

                //using (var compressStream = new MemoryStream())
                //using (var compressor = new GZipStream(compressStream, CompressionMode.Compress))
                //{
                //    await stream.CopyToAsync(compressor);
                //    compressor.Close();
                //    byteArray = compressStream.ToArray();
                //}
                //using (var ms = new MemoryStream(byteArray))
                //    await blob.UploadFromStreamAsync(ms, options);
            }
            else
            {
                await blob.UploadFromStreamAsync(stream, options);
            }
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