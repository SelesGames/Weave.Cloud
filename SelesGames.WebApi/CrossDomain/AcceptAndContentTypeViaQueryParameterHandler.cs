using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.WebApi
{
    public class AcceptAndContentTypeViaQueryParameterHandler : DelegatingHandler
    {
        string queryKey;

        public AcceptAndContentTypeViaQueryParameterHandler(string queryKey)
        {
            this.queryKey = queryKey;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            KeyValuePair<string, string>? queryKVP = request.GetQueryNameValuePairs().FirstOrDefault(o => o.Key == queryKey);

            if (queryKVP != null && queryKVP.HasValue && !string.IsNullOrWhiteSpace(queryKVP.Value.Value))
            {
                var queryValue = queryKVP.Value.Value;

                var requestMethod = request.Method;

                if (requestMethod == HttpMethod.Post || requestMethod == HttpMethod.Put)
                {
                    var contentHeaders = request.Content.Headers;

                    MediaTypeHeaderValue contentType;
                    if (MediaTypeHeaderValue.TryParse(queryValue, out contentType))
                        contentHeaders.ContentType = contentType;
                }

                var accept = request.Headers.Accept;
                if (accept != null)
                {
                    if (accept.Any())
                        accept.Clear();
                    accept.TryParseAdd(queryValue);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
