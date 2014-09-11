using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace SelesGames.WebApi.Handlers
{
    public class StandardExceptionHandler : IExceptionHandler
    {
        class ResponseFactory : IHttpActionResult
        {
            readonly HttpResponseMessage message;

            public ResponseFactory(HttpResponseMessage message)
            {
                this.message = message;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(message);
            }
        }

        public async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            await Task.Delay(0);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var content = new { Exception = context.Exception };
            responseMessage.Content = new ObjectContent<object>(content, context.RequestContext.Configuration.Formatters.First());
            context.Result = new ResponseFactory(responseMessage);
        }
    }
}