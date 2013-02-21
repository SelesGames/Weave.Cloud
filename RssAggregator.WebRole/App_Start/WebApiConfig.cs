using Common.WebApi;
using Ninject.WebApi;
using RssAggregator.WebRole.Controllers;
using RssAggregator.WebRole.Startup;
using SelesGames.WebApi.Compression;
using SelesGames.WebApi.Protobuf;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace RssAggregator.WebRole
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "api/{controller}",
                defaults: new
                {
                    controller = typeof(PingController),
                }
            );

            var jsonFormatter = (JsonMediaTypeFormatter)config.Formatters.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            config.MessageHandlers.Add(new EncodingDelegateHandler());
            config.Formatters.Add(new ProtobufFormatter());

            config.MessageHandlers.Add(new InjectAcceptHandler("application/protobuf"));

            config.DependencyResolver = new NinjectResolver(NinjectKernel.Current);
        }
    }
}
