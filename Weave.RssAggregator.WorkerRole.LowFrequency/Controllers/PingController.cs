using System.Web.Http;

namespace Weave.RssAggregator.WorkerRole.LowFrequency.Controllers
{
    public class PingController : ApiController
    {
        public string Get()
        {
            return "OK";
        }
    }
}
