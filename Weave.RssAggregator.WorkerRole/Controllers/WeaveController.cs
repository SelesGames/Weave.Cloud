using RssAggregator.IconCaching;
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
        readonly FeedCache feedCache;
        readonly NLevelIconUrlCache iconCache;

        public WeaveController(FeedCache cache, NLevelIconUrlCache iconCache)
        {
            this.feedCache = cache;
            this.iconCache = iconCache;
        }

        [HttpGet]
        public Task<FeedResult> Get([FromUri] string feedUri)
        {
            var feedRequest = new FeedRequest { Url = feedUri };
            return GetResultFromRequest(feedRequest);
        }

        [HttpPost]
        public async Task<List<FeedResult>> Get([FromBody] List<FeedRequest> requests, [FromUri] bool fsd = true)
        {
            if (requests == null || !requests.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one FeedRequest object in the message body");

            var temp = await Task.WhenAll(requests.Select(o => GetResultFromRequest(o, fsd)));
            var results = temp.ToList();
            return results;
        }




        #region Helper method containing the logic for the get operation

        async Task<FeedResult> GetResultFromRequest(FeedRequest feedRequest, bool fsd = true)
        {
            FeedResult result = null;

            if (feedCache.ContainsValid(feedRequest.Url))
            {
                result = feedCache.ToFeedResult(feedRequest);
            }
            else
            {
                var requestClient = new RequestClient { RequestTimeout = AppSettings.LowFrequencyHttpWebRequestTimeout };
                result = await requestClient.GetNewsAsync(feedRequest);
            }

            if (result != null && result.News != null && fsd)
            {
                foreach (var newsItem in result.News)
                    newsItem.Description = null;
            }

            var iconUrl = await iconCache.Get(feedRequest.Url);
            result.IconUri = iconUrl;
            return result;
        }

        #endregion
    }
}
