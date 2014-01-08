using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.WebApi.Handlers
{
    public class InjectAcceptEncodingHandler : DelegatingHandler
    {
        readonly StringWithQualityHeaderValue acceptEncoding;

        [Obsolete("No longer used")]
        public bool ClearRequestedAcceptEncoding { get; set; }

        public InjectAcceptEncodingHandler(string acceptEncoding)
        {
            if (!StringWithQualityHeaderValue.TryParse(acceptEncoding, out this.acceptEncoding))
                throw new ArgumentException("invalid encoding in InjectAcceptEncodingHandler constructor");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestedAcceptEncoding = request.Headers.AcceptEncoding;

            if (requestedAcceptEncoding != null && !requestedAcceptEncoding.Any(MatchesAcceptEncoding))
            {
                requestedAcceptEncoding.Add(acceptEncoding);
            }

            return base.SendAsync(request, cancellationToken);
        }

        bool MatchesAcceptEncoding(StringWithQualityHeaderValue headerValue)
        {
            var accept = acceptEncoding.Value;

            return
                headerValue != null &&
                headerValue.Value != null &&
                headerValue.Value.Equals(accept, StringComparison.OrdinalIgnoreCase);
        }
    }
}