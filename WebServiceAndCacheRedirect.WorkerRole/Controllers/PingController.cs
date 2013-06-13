using System.Web.Http;

namespace WebServiceAndCacheRedirect.WorkerRole.Controllers
{
    public class PingController : ApiController
    {
        public string Get()
        {
            return "OK";
        }
    }
}
