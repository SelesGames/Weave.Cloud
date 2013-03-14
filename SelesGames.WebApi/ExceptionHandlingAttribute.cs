using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Common.WebApi
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context == null || context.Exception == null)
                return;

#if DEBUG
            //Log Critical errors
            System.Diagnostics.Debug.WriteLine(context.Exception);
#endif
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(context.Exception.Message),
                ReasonPhrase = "Critical Exception"
            });
        }
    }
}