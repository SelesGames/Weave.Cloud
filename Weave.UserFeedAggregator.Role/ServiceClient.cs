using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.UserFeedAggregator.Role
{
    public class ServiceClient
    {
        const string SERVICE_URL = "http://weave2.cloudapp.net/api/Weave";

        public async Task<List<FeedResult>> GetFeedResultsAsync(List<FeedRequest> outgoingFeedRequests)
        {
            var client = new SelesGames.Rest.Protobuf.ProtobufRestClient { UseGzip = false };
            var results = await client.PostAsync<List<FeedRequest>, List<FeedResult>>(SERVICE_URL, outgoingFeedRequests, CancellationToken.None);
            return results;
        }
    }
}