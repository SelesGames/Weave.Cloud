using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class FeedsSubset : IEnumerable<Feed>
    {
        List<Feed> feeds = new List<Feed>();

        public FeedsSubset(IEnumerable<Feed> feeds)
        {
            this.feeds.AddRange(feeds);
        }

        public async Task Refresh()
        {
            if (feeds == null || !feeds.Any())
                return;

            var client = new NewsServer();
            foreach (var feed in feeds)
                feed.RefreshNews(client);

            client.SendRequests();
            await Task.WhenAll(feeds.Select(o => o.CurrentRefresh));
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
