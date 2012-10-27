using SelesGames.WebApi.Compression;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Weave.Mobilizer.Core.Controllers;

namespace Weave.Mobilizer.Core.Web
{
    public static class ConfigurationExtensions
    {
        public static void Configure(this HttpConfiguration config, IDependencyResolver resolver)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}",
                defaults: new
                {
                    controller = typeof(IPFController),
                }
            );

            config.DependencyResolver = resolver;

            config.MessageHandlers.Add(new EncodingDelegateHandler { ForceCompression = true });
        }
    }
}
