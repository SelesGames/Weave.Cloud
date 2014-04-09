using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.v2.ServiceClients;

namespace Weave.User.BusinessObjects.v2
{
    public class FeedsUpdateMediator
    {
        readonly List<Feed> feeds;
        readonly MasterNewsItemCollection newsCollection;

        public FeedsUpdateMediator(IEnumerable<Feed> feeds, MasterNewsItemCollection newsCollection)
        {
            this.feeds = new List<Feed>(feeds);
            this.newsCollection = newsCollection;
        }

        public async Task Refresh()
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var client = new NewsServer();

            var feedUpdateMediators = feeds
                .Select(o => new FeedUpdateMediator(o, newsCollection, client))
                .ToList();

            foreach (var mediator in feedUpdateMediators)
                mediator.RefreshNews();

            client.SendRequests();

            await Task.WhenAll(feedUpdateMediators.Select(o => o.CurrentRefresh));
        }
    }
}
