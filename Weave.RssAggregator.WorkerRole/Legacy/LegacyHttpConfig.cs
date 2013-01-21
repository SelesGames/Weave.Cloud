using Common.WebApi;
using SelesGames.WebApi.Compression;
using SelesGames.WebApi.Protobuf;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Weave.RssAggregator.WorkerRole.Controllers;

namespace Weave.RssAggregator.WorkerRole.Legacy
{
    public class LegacyHttpConfig : HttpSelfHostConfiguration
    {
        public LegacyHttpConfig(string url)
            : base(url)
        {
            Routes.MapHttpRoute(
                name: "defaultRouting",
                routeTemplate: "{controller}",
                defaults: new
                {
                    controller = typeof(PingController),
                }
            );

            var jsonFormatter = (JsonMediaTypeFormatter)Formatters.First();
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            MessageHandlers.Add(new EncodingDelegateHandler { ForceCompression = true });
            Formatters.Add(new ProtobufFormatter());

            // to support legacy Weave apps which do not send 'Accept' headers
            MessageHandlers.Add(new InjectAcceptHandler("application/protobuf"));
        }

        //class InjectProtobufFilter : ActionFilterAttribute
        //{
        //    public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        //    {
        //        if (actionContext.ControllerContext.Controller is WeaveController)
        //        {
        //            var accept = actionContext.Request.Headers.Accept;
        //            if (accept == null || !accept.Any())
        //                accept.TryParseAdd("application/protobuf");
        //        }
        //        base.OnActionExecuting(actionContext);
        //    }
        //}
    }
}
