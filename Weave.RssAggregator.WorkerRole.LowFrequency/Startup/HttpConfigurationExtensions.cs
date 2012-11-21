using SelesGames.WebApi.Compression;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace Weave.RssAggregator.WorkerRole.LowFrequency
{
    public static class HttpConfigurationExtensions
    {
        public static void Configure(this HttpConfiguration config, IDependencyResolver resolver)
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
