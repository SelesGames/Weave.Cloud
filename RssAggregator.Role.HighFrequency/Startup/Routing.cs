using SelesGames.WebApi.Compression;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Weave.RssAggregator.WorkerRole.HighFrequency.Startup
{
    public class Routing
    {
        public Routing(HttpConfiguration config, IDependencyResolver resolver)
        {
            config.Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "api/{controller}",
                defaults: new
                {
                    controller = "weave",
                }
            );

            config.DependencyResolver = resolver;

            config.MessageHandlers.Add(new EncodingDelegateHandler { ForceCompression = true });
        }
    }
}
