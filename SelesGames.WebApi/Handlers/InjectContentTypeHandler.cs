using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.WebApi.Handlers
{
    public class InjectContentTypeHandler : DelegatingHandler
    {
        readonly MediaTypeHeaderValue contentType;

        public InjectContentTypeHandler(string contentType)
        {
            if (!MediaTypeHeaderValue.TryParse(contentType, out this.contentType))
                throw new ArgumentException("invalid contentType in InjectContentTypeFilter constructor");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestMethod = request.Method;

            if (requestMethod == HttpMethod.Post || requestMethod == HttpMethod.Put)
            {
                var contentHeaders = request.Content.Headers;

                if (contentHeaders.ContentType == null)
                    contentHeaders.ContentType = contentType;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
