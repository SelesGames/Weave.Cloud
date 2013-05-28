using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.User.BusinessObjects.ServiceClients
{
    public class NewsServer
    {
        struct shim
        {
            public FeedRequest Request { get; set; }
            public TaskCompletionSource<FeedResult> TaskSource { get; set; }
        }

        List<shim> updateQueue = new List<shim>();

        public void SendRequests()
        {
            var copy = updateQueue.ToList();
            updateQueue.Clear();
            ProcessQueueViaWeaveServer(copy);
        }

        public Task<FeedResult> GetFeedResultAsync(FeedRequest request)
        {
            var t = new TaskCompletionSource<FeedResult>();
            AddToQueue(request, t);
            return t.Task;
        }

        void AddToQueue(FeedRequest request, TaskCompletionSource<FeedResult> taskSource)
        {
            var x = new shim { Request = request, TaskSource = taskSource };
            updateQueue.Add(x);
        }

        async void ProcessQueueViaWeaveServer(IList<shim> list)
        {
            if (list == null || !list.Any())
                return;

            var shimLookup = list.ToDictionary(o => o.Request.Id);
            var outgoingFeedRequests = shimLookup.Select(o => o.Value.Request).ToList();

            try
            {
                var client = new ServiceClient();
                var feedResults = await client.GetFeedResultsAsync(outgoingFeedRequests).ConfigureAwait(false);

                foreach (var result in feedResults)
                {
                    var shim = shimLookup[result.Id];
                    var request = shim.Request;
                    var taskSource = shim.TaskSource;

                    taskSource.TrySetResult(result);
                }
            }
            catch (Exception exception)
            {
                foreach (var shim in list)
                {
                    shim.TaskSource.TrySetException(exception);
                }
            }
        }
    }
}