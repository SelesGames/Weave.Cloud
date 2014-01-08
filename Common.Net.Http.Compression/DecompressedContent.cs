using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class DecompressedContent : HttpContent
    {
        HttpContent originalContent;
        string encodingType;

        public DecompressedContent(HttpContent content, string encodingType)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (encodingType == null) throw new ArgumentNullException("encodingType");

            originalContent = content;
            this.encodingType = encodingType.ToLowerInvariant();

            if (this.encodingType != "gzip" && this.encodingType != "deflate")
            {
                throw new InvalidOperationException(string.Format("Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", this.encodingType));
            }

            // copy the headers from the original content
            foreach (var header in originalContent.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (var ogStream = await originalContent.ReadAsStreamAsync().ConfigureAwait(false))
            using (var gzip = CreateDecompressedStream(ogStream))
            {
                await gzip.CopyToAsync(stream).ConfigureAwait(false);
                gzip.Close();
                ogStream.Close();
            }
        }

        Stream CreateDecompressedStream(Stream stream)
        {
            if (encodingType == "gzip")
            {
                return new GZipStream(stream, CompressionMode.Decompress, leaveOpen: false);
            }
            else if (encodingType == "deflate")
            {
                return new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: false);
            }
            else
            {
                throw new ArgumentException(string.Format("unsupported encodingType: {0}", encodingType));
            }
        }
    }
}
