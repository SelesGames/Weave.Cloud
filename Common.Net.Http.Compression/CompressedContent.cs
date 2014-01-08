using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class CompressedContent : HttpContent
    {
        HttpContent originalContent;
        string encodingType;

        public CompressedContent(HttpContent content, string encodingType)
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

            this.Headers.ContentEncoding.Add(encodingType);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (var compressedStream = CreateCompressedStream(stream, encodingType))
            {
                await originalContent.CopyToAsync(compressedStream).ConfigureAwait(false);
                compressedStream.Flush();
                compressedStream.Close();
            }
        }

        Stream CreateCompressedStream(Stream stream, string encodingType)
        {
            if (encodingType == "gzip")
            {
                return new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
            }
            else if (encodingType == "deflate")
            {
                return new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
            }
            else
            {
                throw new ArgumentException(string.Format("unsupported encodingType: {0}", encodingType));
            }
        }
    }
}




//protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
//{
//    var compressedStream = CreateCompressedStream(stream, encodingType);

//    return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
//    {
//        if (compressedStream != null)
//        {
//            compressedStream.Flush();
//            compressedStream.Close();
//            compressedStream.Dispose();
//        }
//    });
//}
