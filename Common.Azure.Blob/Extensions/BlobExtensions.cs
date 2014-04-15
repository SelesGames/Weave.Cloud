using Common.Azure.Compression;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Azure.Blob
{
    public static class BlobExtensions
    {
        static AccessCondition EmptyAccessCondition = AccessCondition.GenerateEmptyCondition();




        #region Container Management

        public static Task<bool> CreateContainer(
            this AzureBlobClient client,
            string containerName,
            BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Off,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var container = client.GetContainerHandle(containerName);
            return container.CreateIfNotExistsAsync(accessType, options, operationContext);
        }

        public static Task DeleteContainer(
            this AzureBlobClient client,
            string containerName,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var container = client.GetContainerHandle(containerName);
            return container.DeleteAsync(accessCondition, options, operationContext);
        }

        #endregion




        #region Get Blob

        public static async Task<BlobContent> GetBlobContent(
            this AzureBlobClient client,
            string container,
            string blobName,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var blob = client.GetBlobHandle(container, blobName);

            var ms = new MemoryStream();
            await blob.DownloadToStreamAsync(ms, accessCondition, options, operationContext).ConfigureAwait(false);
            ms.Position = 0;

            if ("gzip".Equals(blob.Properties.ContentEncoding, StringComparison.OrdinalIgnoreCase))
            {
                byte[] byteArray = ms.ToArray();

                //var magicNumber = BitConverter.ToString(byteArray.Take(3).ToArray());
                //if (magicNumber.Equals("1F-8B-08"))
                //{
                ms.Dispose();
                var streamContent = await byteArray.DecompressToStream().ConfigureAwait(false);
                return BlobContent.Create(blob, streamContent);
                //}
            }
            return BlobContent.Create(blob, ms);
        }

        public static Task<BlobContent> GetBlobContent(
            this AzureBlobClient client,
            string container,
            string blobName,
            RequestProperties properties)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            return client.GetBlobContent(container, blobName,
                options: new BlobRequestOptions
                {
                    MaximumExecutionTime = properties.RequestTimeOut
                });
        }

        #endregion




        #region Save Blob

        public static async Task SaveBlobContent(
            this AzureBlobClient client,
            string container,
            string blobName,
            Stream stream,
            BlobProperties blobProperties = null,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var blob = client.GetBlobHandle(container, blobName);

            CopyBlobProperties(source: blobProperties, destination: blob.Properties);

            if (blob.HasRequestedGzipEncoding())
            {
                var compressedArray = await stream.CompressToByteArray();
                using (var compressedStream = new MemoryStream(compressedArray))
                {
                    await blob.UploadFromStreamAsync(compressedStream, accessCondition, options, operationContext);
                }
            }
            else
            {
                await blob.UploadFromStreamAsync(stream, accessCondition, options, operationContext);
            }
        }

        public static Task SaveBlobContent(
            this AzureBlobClient client,
            string container,
            string blobName,
            Stream stream,
            WriteRequestProperties properties)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            var blobProperties = new BlobProperties
            {
                CacheControl = properties.CacheControl,
                //ContentEncoding = properties.ContentEncoding,
                ContentEncoding = (properties.UseCompression.HasValue && properties.UseCompression.Value) ? "gzip" : null,
                ContentLanguage = properties.ContentLanguage,
                ContentMD5 = properties.ContentMD5,
                ContentType = properties.ContentType,
            };

            var requestOptions = new BlobRequestOptions
            {
                MaximumExecutionTime = properties.RequestTimeOut,
            };

            return client.SaveBlobContent(container, blobName, stream, 
                blobProperties: blobProperties,
                options: requestOptions);
        }

        #endregion




        #region Delete Blob

        public static Task DeleteBlob(
            this AzureBlobClient client,
            string container, 
            string blobName,
            DeleteSnapshotsOption deleteSnapshotsOption = DeleteSnapshotsOption.None,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var blob = client.GetBlobHandle(container, blobName);
            return blob.DeleteAsync(deleteSnapshotsOption, accessCondition, options, operationContext);
        }

        public static Task DeleteBlob(
            this AzureBlobClient client,
            string container,
            string blobName,
            RequestProperties properties)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            return client.DeleteBlob(container, blobName,
                options: new BlobRequestOptions
                {
                    MaximumExecutionTime = properties.RequestTimeOut
                });
        }

        #endregion




        #region Private static helper methods

        static void CopyBlobProperties(BlobProperties source, BlobProperties destination)
        {
            if (source == null || destination == null)
                return;

            destination.CacheControl = source.CacheControl;
            destination.ContentEncoding = source.ContentEncoding;
            destination.ContentLanguage = source.ContentLanguage;
            destination.ContentMD5 = source.ContentMD5;
            destination.ContentType = source.ContentType;
        }

        static bool HasRequestedGzipEncoding(this ICloudBlob blob)
        {
            return 
                blob != null &&
                blob.Properties != null &&
                blob.Properties.ContentEncoding != null &&
                blob.Properties.ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
