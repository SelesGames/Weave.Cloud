using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.WebApi.Compression
{
    public class EncodingDelegateHandler : DelegatingHandler
    {
        public List<string> OptOut { get; set; }
        public bool ForceCompression { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((responseToCompleteTask) =>
            {
                HttpResponseMessage response = responseToCompleteTask.Result;

                var acceptEncoding = response.RequestMessage.Headers.AcceptEncoding;
                var absoluteUrl = request.RequestUri.AbsolutePath.Substring(1);

                if (response.IsSuccessStatusCode && 
                    !(OptOut != null && OptOut.Any(o => absoluteUrl.StartsWith(o, StringComparison.OrdinalIgnoreCase))))
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
