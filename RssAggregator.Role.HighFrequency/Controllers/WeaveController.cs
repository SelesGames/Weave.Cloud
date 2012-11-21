using System.Collections.Generic;
using System.Web.Http;
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

        public HighFrequencyFeed Get(string url)
        {
            if (cache.Contains(url))
                return cache.Get(url);

            else throw new KeyNotFoundException(string.Format("{0} was not a high frequency feed and not found in cache"));
        }
    }
}
