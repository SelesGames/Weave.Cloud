using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Weave.Mobilizer.Core.Cache;
using Weave.Readability;

namespace Weave.Mobilizer.Core.Controllers
{
    public class IPFController : ApiController
    {
        ReadabilityCache cache;

        public IPFController(ReadabilityCache cache)
        {
            this.cache = cache;
        }

        public Task<ReadabilityResult> Get(string url)
        {
            return cache.Get(HttpUtility.UrlEncode(url));
        }

        public Task Post(string url, [FromBody] object article)
        {
            return null;
        }
    }
}
