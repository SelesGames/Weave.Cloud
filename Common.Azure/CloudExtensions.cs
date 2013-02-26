using System.IO;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.StorageClient
{
    internal static class CloudExtensions
    {
        public static Task DeleteAsync(this CloudBlob blob)
        {
            return Task.Factory.FromAsync(blob.BeginDelete, blob.EndDelete, null);
        }

        public static Task DownloadToStreamAsync(this CloudBlob blob, Stream stream, BlobRequestOptions options)
        {
            return Task.Factory.FromAsync<Stream, BlobRequestOptions>(blob.BeginDownloadToStream, blob.EndDownloadToStream, stream, options, null);
        }

        public static Task UploadFromStreamAsync(this CloudBlob blob, Stream stream, BlobRequestOptions options)
        {
            return Task.Factory.FromAsync<Stream, BlobRequestOptions>(blob.BeginUploadFromStream, blob.EndUploadFromStream, stream, options, null);
        }
    }
}
