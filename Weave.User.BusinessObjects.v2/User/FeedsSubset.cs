using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.v2.ServiceClients;

namespace Weave.User.BusinessObjects.v2
{
    public class FeedsSubset : IEnumerable<Feed>
    {
        List<Feed> feeds;

        public FeedsSubset(IEnumerable<Feed> feeds)
        {
            this.feeds = new List<Feed>(feeds);
        }

        public async Task Refresh(MasterNewsItemCollection newsCollection)
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




        #region IEnumerable interface implementation

        public IEnumerator<Feed> GetEnumerator()
        {
            return feeds.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return feeds.GetEnumerator();
        }

        #endregion
    }
}
