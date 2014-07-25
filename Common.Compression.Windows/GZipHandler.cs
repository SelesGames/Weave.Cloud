using Common.Compression;
using System.IO;
using System.IO.Compression;

namespace Common.Compression.Windows
{
    public class GZipHandler : CompressionHandler
    {
        public GZipHandler()
        {
            SupportedEncodings.Add("gzip");
        }

        public override Stream Compress(Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Compress, leaveOpen: false);
        }

        public override Stream Decompress(Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Decompress, leaveOpen: false);
        }
    }
}
