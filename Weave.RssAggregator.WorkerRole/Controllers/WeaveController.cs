using SelesGames.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.LowFrequency;
using Weave.RssAggregator.WorkerRole.Startup;

namespace Weave.RssAggregator.WorkerRole.Controllers
{
    public class WeaveController : ApiController
    {
        readonly FeedCache cache;

        public WeaveController(FeedCache cache)
        {
            this.cache = cache;
        }

        public Task<FeedResult> Get(string feedUri)
        {
            var feedRequest = new FeedRequest { Url = feedUri };
            return GetResultFromRequest(feedRequest);
        }

        [HttpPost]
        public async Task<List<FeedResult>> Get([FromBody] List<FeedRequest> requests, bool fsd = true)
        {
            if (requests == null || !requests.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one FeedRequest object in the message body");

            var temp = await Task.WhenAll(requests.Select(o => GetResultFromRequest(o, fsd)));
            var results = temp.ToList();

            //var highFrequencyFeeds = requests.Where(o => cache.ContainsValid(o.Url)).ToList();
            //var lowFrequencyFeeds = requests.Except(highFrequencyFeeds).ToList();

            //var lowFrequencyResults = await Task.WhenAll(lowFrequencyFeeds.Select(o => o.GetNewsAsync(AppSettings.LowFrequencyHttpWebRequestTimeout)));
            //var highFrequencyResults = highFrequencyFeeds.Select(o => cache.ToFeedResult(o)).ToArray();

            //var results = new List<FeedResult>(lowFrequencyResults.Length + highFrequencyResults.Length);
            //results.AddRange(lowFrequencyResults);
            //results.AddRange(highFrequencyResults);

            //if (fsd && results != null)
            //{
            //    foreach (var r in results.Where(o => o.News != null))
            //        foreach (var newsItem in r.News)
            //            newsItem.Description = null;
            //}

            return results;
        }

        public async Task<FeedResult> GetResultFromRequest(FeedRequest feedRequest, bool fsd = true)
        {
            FeedResult result = null;

            if (cache.ContainsValid(feedRequest.Url))
            {
                result = cache.ToFeedResult(feedRequest);
            }
            else
            {
                result = await feedRequest.GetNewsAsync(AppSettings.LowFrequencyHttpWebRequestTimeout);
            }

            if (result != null && result.News != null && fsd)
            {
                foreach (var newsItem in result.News)
                    newsItem.Description = null;
            }
            return result;
        }
    }
}
