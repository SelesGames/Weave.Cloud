using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class HttpClientCompressionHandler : HttpClientHandler
    {
        public override bool SupportsAutomaticDecompression
        {
            get { return false; }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith(o => UnzipContent(o), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        HttpResponseMessage UnzipContent(Task<HttpResponseMessage> o)
        {
            HttpResponseMessage response = o.Result;

            if (response.IsSuccessStatusCode)
            {
                var contentEncoding = response.Content.Headers.ContentEncoding;

                if (contentEncoding != null && contentEncoding.Any())
                {
                    string encodingType = contentEncoding.First();

                    if (!encodingType.Equals("identity", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Content = new DecompressedContent(response.Content, encodingType);
                    }
                }
            }

            return response;
        }
    }
}
