using System;
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
        readonly NewsItemStateCache stateCache;

        public FeedsUpdateMediator(IEnumerable<Feed> feeds, MasterNewsItemCollection newsCollection, NewsItemStateCache stateCache)
        {
            if (feeds == null) throw new ArgumentNullException("feeds");
            if (newsCollection == null) throw new ArgumentNullException("news");
            if (stateCache == null) throw new ArgumentNullException("stateCache");

            this.feeds = new List<Feed>(feeds);
            this.newsCollection = newsCollection;
            this.stateCache = stateCache;
        }

        public async Task Refresh()
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var client = new NewsServer();

            var feedUpdateMediators = feeds
                .Select(o => new FeedUpdateMediator(o, newsCollection, stateCache, client))
                .ToList();

            foreach (var mediator in feedUpdateMediators)
                mediator.RefreshNews();

            client.SendRequests();

            await Task.WhenAll(feedUpdateMediators.Select(o => o.CurrentRefresh));
        }
    }
}
