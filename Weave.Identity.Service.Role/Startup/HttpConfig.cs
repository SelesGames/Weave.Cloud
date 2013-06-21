using Common.Net.Http.Compression;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace Weave.Identity.Service.WorkerRole.Startup
{
    public class HttpConfig : HttpSelfHostConfiguration
    {
        public HttpConfig(string url)
            : base(url)
        {
            Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = (JsonMediaTypeFormatter)Formatters.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            MessageHandlers.Add(new EncodingDelegateHandler());
        }
    }
}
