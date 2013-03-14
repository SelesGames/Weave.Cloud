using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Common.Azure.Compression
{
    internal static class MemoryStreamExtensions
    {
        public static async Task<MemoryStream> Compress(this Stream stream)
        {
            byte[] byteArray;

            using (var compressStream = new MemoryStream())
            using (var compressor = new GZipStream(compressStream, CompressionMode.Compress))
            {
                await stream.CopyToAsync(compressor).ConfigureAwait(false);
                compressor.Close();
                byteArray = compressStream.ToArray();
            }

            return new MemoryStream(byteArray);
        }

        //public static async Task<MemoryStream> Decompress(this MemoryStream stream)
        //{
        //    byte[] byteArray = stream.ToArray();

        //    ms.Dispose();
        //    ms = new MemoryStream();

        //    using (var compressStream = new MemoryStream(byteArray))
        //    using (var decompressor = new GZipStream(compressStream, CompressionMode.Decompress))
        //        decompressor.CopyTo(ms);

        //    ms.Position = 0;
        //}
    }
}