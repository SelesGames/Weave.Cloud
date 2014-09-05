using Common.Azure.Blob;
using Common.Azure.SmartBlobClient;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Blob
{
    public static class SmartBlobClientExtensions
    {
        public static async Task<BlobResult<T>> Get<T>(this SmartBlobClient client,
            string container,
            string blobName,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            try
            {
                using (var content = await client.GetBlobContent(container, blobName, accessCondition, options, operationContext))
                {
                    var response = new BlobResponse(content, client.Formatters);
                    var result = await response.Read<T>();
                    result.BlobName = blobName;
                    return result;
                }
            }
            catch (StorageException storageException)
            {
                return new BlobResult<T> { BlobName = blobName, StorageException = storageException };
            }
        }

        public static async Task Save<T>(this SmartBlobClient client,
            string container,
            string blobName,
            T obj,
            BlobProperties blobProperties = null,
            AccessCondition accessCondition = null,
            BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            blobProperties = blobProperties ?? new BlobProperties();

            blobProperties.ContentEncoding = blobProperties.ContentEncoding ??
                ((client.UseCompressionByDefault.HasValue && client.UseCompressionByDefault.Value) ? "gzip" : null);

            blobProperties.ContentType = blobProperties.ContentType ?? client.DefaultContentType;
            var formatter = client.GetBestTypeFormatterForContentType<T>(blobProperties.ContentType);

            using (var ms = new MemoryStream())
            {
                await formatter.WriteToStreamAsync(typeof(T), obj, ms, null, null);
                ms.Position = 0;
                await client.SaveBlobContent(container, blobName, ms, blobProperties, accessCondition, options, operationContext);
            }
        }
    }
}
