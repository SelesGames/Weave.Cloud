using SelesGames.Rest.JsonDotNet;
using SelesGames.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.HighFrequency;
using Weave.RssAggregator.WorkerRole.LowFrequency.Startup;

namespace Weave.Mobilizer.Core.Controllers
{
    public class WeaveController : ApiController
    {
        readonly HighFrequencyFeedCache cache;

        public WeaveController(HighFrequencyFeedCache cache)
        {
            this.cache = cache;
        }

        [HttpPost]
        public async Task<List<FeedResult>> Get([FromBody] List<FeedRequest> requests, bool fsd)
        {
            if (requests == null || !requests.Any())
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.BadRequest, 
                    "You must send at least one FeedRequest object in the message body");

            var highFrequencyRequests = requests.Where(o => cache.Contains(o.Url)).ToList();
            var lowFrequencyRequests = requests.Except(highFrequencyRequests).ToList();

            var lowFrequencyResults = await Task
                .WhenAll(lowFrequencyRequests.Select(o => o.GetNewsAsync(AppSettings.LowFrequencyHttpWebRequestTimeout)));

            var client = new JsonDotNetRestClient<HighFrequencyFeed>();
            var highFrequencyFeeds = await Task.WhenAll(highFrequencyRequests.Select(o => GetFeedAsync(client, o)));
            var highFrequencyResults = highFrequencyFeeds.Select(o => o.Item1.ToFeedResult(o.Item2)).ToArray();

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

        async Task<Tuple<HighFrequencyFeed, FeedRequest>> GetFeedAsync(JsonDotNetRestClient<HighFrequencyFeed> client, FeedRequest request)
        {
            var client2 = ChannelFactory<IWeaveControllerService>
                .CreateChannel(
                    new NetTcpBinding(SecurityMode.None), 
                    new EndpointAddress(string.Format("net.tcp://{0}/api/weave", AppSettings.InternalHighFrequencyEndpoint)));

            //var url = string.Format("{0}/api/weave?url={1}", AppSettings.InternalHighFrequencyEndpoint, request.Url);
            //var hfFeed = await client.GetAsync(url, CancellationToken.None);

            var hfFeed = await client2.Get(request.Url);
            return Tuple.Create(hfFeed, request);
        }

        public class HFClient : ClientBase<IWeaveControllerService>, IWeaveControllerService
        {
            public Task<HighFrequencyFeed> Get(string url)
            {
                return Channel.Get(url);
            }
        }

        [ServiceContract]
        public interface IWeaveControllerService
        {
            [OperationContract]
            Task<HighFrequencyFeed> Get(string url);
        }
    }
}
