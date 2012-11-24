using SelesGames.WebApi.Compression;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Weave.RssAggregator.WorkerRole.LowFrequency.Startup
{
    public class Routing
    {
        public Routing(HttpConfiguration config, IDependencyResolver resolver)
        {
            config.Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "{controller}",
                defaults: new
                {
                    controller = typeof(Weave.RssAggregator.WorkerRole.LowFrequency.Controllers.PingController),
                }
            );

            config.DependencyResolver = resolver;

            config.MessageHandlers.Add(new EncodingDelegateHandler { ForceCompression = true });
        }
    }
}
