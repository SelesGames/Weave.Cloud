using ImageResizer.Role.Controllers;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace ImageResizer.Role.Startup
{
    public class HttpConfig : HttpSelfHostConfiguration
    {
        public HttpConfig(string url)
            : base(url)
        {
            Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "api/{controller}",
                defaults: new
                {
                    controller = typeof(ImageController),
                }
            );
        }
    }
}
