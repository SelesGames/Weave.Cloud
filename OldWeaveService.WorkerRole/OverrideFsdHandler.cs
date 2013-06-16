using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OldWeaveService.WorkerRole
{
    public class OverrideFsdHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            var queryParameters = uri.ParseQueryString();

            if (queryParameters["fsd"] == "true")
                ;
            else
            {
                //request.RequestUri = new UriBuilder(uri).AddParameter("fsd", "false").Uri;
                //var errorResponse = request.CreateErrorResponse(HttpStatusCode.Conflict, "fsd no longer supported");
                //throw ResponseHelper.CreateResponseException(HttpStatusCode.Conflict, "fsd no longer supported");
                //return Task.FromResult(errorResponse);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict) { ReasonPhrase = "fsd no longer supported" });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
