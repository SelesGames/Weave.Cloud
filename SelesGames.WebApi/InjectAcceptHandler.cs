using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.WebApi
{
    public class InjectAcceptHandler : DelegatingHandler
    {
        readonly MediaTypeWithQualityHeaderValue accept;

        public InjectAcceptHandler(string accept)
        {
            if (!MediaTypeWithQualityHeaderValue.TryParse(accept, out this.accept))
                throw new ArgumentException("invalid contentType in InjectAcceptHandler constructor");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var acceptHeader = request.Headers.Accept;

            if (acceptHeader != null && !acceptHeader.Any())
                acceptHeader.Add(accept);

            return base.SendAsync(request, cancellationToken);              
        }
    }
}
