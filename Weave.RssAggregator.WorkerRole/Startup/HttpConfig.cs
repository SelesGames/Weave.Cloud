using Common.Net.Http.Compression;
using Common.WebApi;
using SelesGames.WebApi.Protobuf;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Weave.RssAggregator.WorkerRole.Controllers;

namespace Weave.RssAggregator.WorkerRole.Startup
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
                    controller = typeof(PingController),
                }
            );

            var jsonFormatter = (JsonMediaTypeFormatter)Formatters.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            MessageHandlers.Add(new EncodingDelegateHandler());
            Formatters.Add(new ProtobufFormatter());

            MessageHandlers.Add(new InjectAcceptHandler("application/protobuf"));
        }
    }
}
