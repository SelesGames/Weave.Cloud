using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Weave.Mobilizer.Core.Web
{
    public static class ConfigurationExtensions
    {
        public static void Configure(this HttpConfiguration config, IDependencyResolver resolver)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/IPF"
                //defaults: new
                //{
                //    id = RouteParameter.Optional,
                //    controller = typeof(GameController),
                //}
            );

            config.DependencyResolver = resolver;
        }
    }
}
