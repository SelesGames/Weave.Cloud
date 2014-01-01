using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Common.Azure.Compression
{
    internal static class MemoryStreamExtensions
    {
        public static async Task<Stream> DecompressToStream(this byte[] byteArray)
        {
            MemoryStream returnStream = new MemoryStream();

            using (var compressStream = new MemoryStream(byteArray))
            using (var decompressor = new GZipStream(compressStream, CompressionMode.Decompress))
            {
                await decompressor.CopyToAsync(returnStream);
            }

            returnStream.Position = 0;
            return returnStream;
        }

        public static async Task<byte[]> CompressToByteArray(this Stream stream)
        {
            byte[] byteArray;

            using (var compressStream = new MemoryStream())
            using (var compressor = new GZipStream(compressStream, CompressionMode.Compress))
            {
                await stream.CopyToAsync(compressor).ConfigureAwait(false);
                compressor.Close();
                byteArray = compressStream.ToArray();
            }

            return byteArray;
        }
    }
}