using SelesGames.WebApi;
using System.Net;
using System.Web.Http;
using Weave.RssAggregator.HighFrequency;

namespace Weave.Mobilizer.Core.Controllers
{
    public class WeaveController : ApiController
    {
        readonly HighFrequencyFeedCache cache;

        public WeaveController(HighFrequencyFeedCache cache)
        {
            this.cache = cache;
        }

        public HighFrequencyFeed Get(string url)
        {
            if (cache.Contains(url))
                return cache.Get(url);

            else
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.NotFound, 
                    string.Format("{0} was not a high frequency feed and not found in cache"));
        }
    }
}
