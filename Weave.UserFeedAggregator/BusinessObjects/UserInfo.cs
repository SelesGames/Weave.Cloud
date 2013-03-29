using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class UserInfo
    {
        public Guid Id { get; set; }
        public List<Feed> Feeds { get; set; }

        public Task RefreshAllFeeds()
        {
            return RefreshFeeds(Feeds);
        }

        public async Task RefreshFeedsMatchingIds(IEnumerable<Guid> feedIds)
        {
            if (feedIds == null || !feedIds.Any())
                return;

            var feeds = Feeds.Join(feedIds, o => o.Id, x => x, (o, x) => o).ToList();
            await RefreshFeeds(feeds);
        }




        #region helper methods

        async Task RefreshFeeds(List<Feed> feeds)
        {
            if (feeds == null || !feeds.Any())
                return;

            var client = new NewsServer();
            foreach (var feed in feeds)
                feed.RefreshNews(client);

            client.SendRequests();
            await Task.WhenAll(feeds.Select(o => o.CurrentRefresh));
        }

        #endregion
    }
}
