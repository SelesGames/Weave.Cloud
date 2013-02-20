using System.Web.Http;

namespace RssAggregator.WebRole.Controllers
{
    public class PingController : ApiController
    {
        public string Get()
        {
            return "OK";
        }
    }
}
