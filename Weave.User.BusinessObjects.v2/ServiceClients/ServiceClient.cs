using SelesGames.HttpClient;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.User.BusinessObjects.v2.ServiceClients
{
    public class ServiceClient
    {
        const string SERVICE_URL = "http://weave-aggregator.cloudapp.net/api/Weave";

        public async Task<List<FeedResult>> GetFeedResultsAsync(List<FeedRequest> outgoingFeedRequests)
        {
            var client = new SmartHttpClient(ContentEncoderSettings.Protobuf, CompressionSettings.None);
            var results = await client.PostAsync<List<FeedRequest>, List<FeedResult>>(SERVICE_URL, outgoingFeedRequests, CancellationToken.None);
            return results;
        }
    }
}