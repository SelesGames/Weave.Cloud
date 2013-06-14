using System.Web.Http;
using System.Web.Http.SelfHost;
using WebServiceAndCacheRedirect.WorkerRole.Controllers;

namespace WebServiceAndCacheRedirect.WorkerRole.Startup
{
    public class HttpConfig : HttpSelfHostConfiguration
    {
        public HttpConfig(string url)
            : base(url)
        {
            Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "{controller}",
                defaults: new
                {
                    controller = typeof(PingController),
                }
            );
        }
    }
}
