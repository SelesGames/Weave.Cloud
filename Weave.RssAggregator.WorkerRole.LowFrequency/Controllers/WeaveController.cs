﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core;
using Weave.RssAggregator.Core.DTOs.Incoming;
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

        [HttpPost]
        public async Task<List<FeedResult>> Get([FromBody] List<FeedRequest> requests, bool fsd)
        {
            var highFrequencyFeeds = requests.Where(o => cache.Contains(o.Url)).ToList();
            var lowFrequencyFeeds = requests.Except(highFrequencyFeeds).ToList();

            var lowFrequencyResults = await Task.WhenAll(lowFrequencyFeeds.Select(o => o.GetNewsAsync(AppSettings.LowFrequencyHttpWebRequestTimeout)));
            var highFrequencyResults = highFrequencyFeeds.Select(o => cache.ToFeedResult(o)).ToArray();

            var results = new List<FeedResult>(lowFrequencyResults.Length + highFrequencyResults.Length);
            results.AddRange(lowFrequencyResults);
            results.AddRange(highFrequencyResults);

            if (fsd && results != null)
            {
                foreach (var r in results.Where(o => o.News != null))
                    foreach (var newsItem in r.News)
                        newsItem.Description = null;
            }

            return results;
        }
    }
}
