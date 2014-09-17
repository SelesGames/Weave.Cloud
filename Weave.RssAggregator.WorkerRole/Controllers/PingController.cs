using System.Web.Http;

namespace Weave.FeedUpdater.Service.Role.Controllers
{
    public class PingController : ApiController
    {
        public string Get()
        {
            return "OK";
        }
    }
}
