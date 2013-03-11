using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.WebApi.Compression
{
    public class EncodingDelegateHandler : DelegatingHandler
    {
        public bool ForceCompression { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith(responseToCompleteTask =>
            {
                HttpResponseMessage response = responseToCompleteTask.Result;

                var acceptEncoding = response.RequestMessage.Headers.AcceptEncoding;

                if (response.IsSuccessStatusCode)
                {
                    if (ForceCompression)
                    {
                        response.Content = new CompressedContent(response.Content, "gzip");
                    }
                    else if (acceptEncoding != null && acceptEncoding.Any())
                    {
                        string encodingType = acceptEncoding.First().Value;

                        if (!encodingType.Equals("identity", StringComparison.OrdinalIgnoreCase))
                        {
                            response.Content = new CompressedContent(response.Content, encodingType);
                        }
                    }
                }

                return response;
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
