using SelesGames.WebApi.Compression;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using Weave.Mobilizer.Core.Controllers;

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

            var jsonFormatter = (JsonMediaTypeFormatter)config.Formatters.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;


            config.DependencyResolver = resolver;

            config.MessageHandlers.Add(new EncodingDelegateHandler { ForceCompression = true });
            config.Formatters.Add(new SelesGames.WebApi.Protobuf.ProtobufFormatter());
            
            // to support legacy Weave apps which do not send 'Accept' headers
            config.Filters.Add(new InjectProtobufFilter());
        }
    }

    public class InjectProtobufFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext.ControllerContext.Controller is WeaveController)
            {
                var accept = actionContext.Request.Headers.Accept;
                if (accept == null || !accept.Any())
                    accept.TryParseAdd("application/protobuf");
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
