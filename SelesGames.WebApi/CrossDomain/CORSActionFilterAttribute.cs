using System.Net.Http;
using System.Web.Http.Filters;

namespace Common.WebApi
{
    public class CORSActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext.Request.Method == HttpMethod.Options)
            {
                // do nothing let IIS deal with reply!
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            else
            {
                base.OnActionExecuting(actionContext);
            }
        }
    }
}
