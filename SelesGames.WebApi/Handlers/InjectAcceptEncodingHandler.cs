using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.WebApi.Handlers
{
    public class InjectAcceptEncodingHandler : DelegatingHandler
    {
        readonly StringWithQualityHeaderValue acceptEncoding;

        public bool ClearRequestedAcceptEncoding { get; set; }

        public InjectAcceptEncodingHandler(string acceptEncoding)
        {
            if (!StringWithQualityHeaderValue.TryParse(acceptEncoding, out this.acceptEncoding))
                throw new ArgumentException("invalid encoding in InjectAcceptEncodingHandler constructor");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var acceptEncodingHeader = request.Headers.AcceptEncoding;

            if (acceptEncodingHeader != null)
            {
                if (ClearRequestedAcceptEncoding)
                    acceptEncodingHeader.Clear();

                //if (!acceptEncodingHeader.Any())
                acceptEncodingHeader.Add(acceptEncoding);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}