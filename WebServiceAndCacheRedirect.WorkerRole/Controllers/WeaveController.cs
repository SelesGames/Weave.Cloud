using SelesGames.WebApi;
using System;
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
        string location = "http://weave2.cloudapp.net";
        readonly FeedCache cache;

        public WeaveController(FeedCache cache)
        {
            this.cache = cache;
        }

        public Task<FeedResult> Get(string feedUri)
        {
            //var feedRequest = new FeedRequest { Url = feedUri };
            //return GetResultFromRequest(feedRequest);

            var pathAndQuery = Request.RequestUri.PathAndQuery;
            var movedTo = location + pathAndQuery;
            var ex = ResponseHelper.CreateResponseException(HttpStatusCode.MovedPermanently, null);
            ex.Response.Headers.Location = new Uri(movedTo, UriKind.Absolute);
            throw ex;
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
                var requestClient = new RequestClient { RequestTimeout = AppSettings.LowFrequencyHttpWebRequestTimeout };
                result = await requestClient.GetNewsAsync(feedRequest);
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
