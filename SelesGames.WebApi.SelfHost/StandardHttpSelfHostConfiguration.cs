using Common.Net.Http.Compression;
using SelesGames.WebApi.Protobuf;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace SelesGames.WebApi.SelfHost
{
    public class StandardHttpSelfHostConfiguration : HttpConfiguration
    {
        public StandardHttpSelfHostConfiguration()
        {
            Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "api/{controller}/{action}",
                defaults: new 
                { 
                    id = RouteParameter.Optional,
                    action = RouteParameter.Optional,
                }
            );

            var jsonFormatter = (JsonMediaTypeFormatter)Formatters.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            MessageHandlers.Add(new WebApiCompressionHandler());
            Formatters.Add(new ProtobufFormatter());

            //Services.Replace(typeof(IExceptionHandler), new StandardExceptionHandler());
        }
    }
}
