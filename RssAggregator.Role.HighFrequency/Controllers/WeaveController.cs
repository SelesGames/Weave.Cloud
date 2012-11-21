using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.HighFrequency;

namespace Weave.Mobilizer.Core.Controllers
{
    public class WeaveController : ApiController
    {
        readonly HighFrequencyFeedRssCache cache;

        public WeaveController(HighFrequencyFeedRssCache cache)
        {
            this.cache = cache;
        }

        public Task<List<FeedResult>> Get(string url)
        {
            //if (cache.Contains(url))
            return null;
        }
    }
}
