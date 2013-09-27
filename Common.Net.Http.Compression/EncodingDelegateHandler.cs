using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Net.Http.Compression
{
    public class EncodingDelegateHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith(responseToCompleteTask =>
            {
                HttpResponseMessage response = responseToCompleteTask.Result;

                if (response.HasEncodeableContent())
                {
                    var acceptEncoding = response.RequestMessage.Headers.AcceptEncoding;

                    if (acceptEncoding != null && acceptEncoding.Any())
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

    internal static class HttpResponseMessageExtensions
    {
        public static bool HasEncodeableContent(this HttpResponseMessage response)
        {
            return
                response.IsSuccessStatusCode &&
                response.Content != null &&
                response.RequestMessage != null &&
                response.RequestMessage.Headers != null;
        }
    }
}
