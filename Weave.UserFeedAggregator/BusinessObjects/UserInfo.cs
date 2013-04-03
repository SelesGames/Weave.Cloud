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

        public void AddFeed(Feed feed)
        {
            if (Feeds == null || !Feeds.Any() || feed == null)
                return;

            feed.EnsureGuidIsSet();

            // if any existing feed has a matching Id, don't add it
            if (Feeds.Any(o => o.Id.Equals(feed.Id)))
                return;

            Feeds.Add(feed);
        }

        public void RemoveFeed(Feed feed)
        {
            if (Feeds == null || !Feeds.Any() || feed == null)
                return;

            feed.EnsureGuidIsSet();

            RemoveFeed(feed.Id);
        }

        public void RemoveFeed(Guid feedId)
        {
            if (Feeds == null || !Feeds.Any())
                return;

            var matching = Feeds.FirstOrDefault(o => o.Id.Equals(feedId));
            if (matching != null)
            {
                Feeds.Remove(matching);
            }
        }

        public void UpdateFeed(Feed feed)
        {
            if (Feeds == null || !Feeds.Any() || feed == null)
                return;

            feed.EnsureGuidIsSet();

            var matching = Feeds.FirstOrDefault(o => o.Id.Equals(feed.Id));
            if (matching != null)
            {
                // the only 3 fields the user can change are category, feed name, and article viewing type
                matching.Category = feed.Category;
                matching.FeedName = feed.FeedName;
                matching.ArticleViewingType = feed.ArticleViewingType;
            }
        }

        public void MarkNewsItemRead(Guid feedId, Guid newsItemId)
        {
            ToggleMarkNewsItemRead(feedId, newsItemId, true);
        }

        public void MarkNewsItemUnread(Guid feedId, Guid newsItemId)
        {
            ToggleMarkNewsItemRead(feedId, newsItemId, false);
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

        void ToggleMarkNewsItemRead(Guid feedId, Guid newsItemId, bool isRead)
        {
            if (Feeds == null || !Feeds.Any())
                return;

            var newsItem = Feeds
                .Where(o => o.Id.Equals(feedId))
                .SelectMany(o => o.News)
                .FirstOrDefault(o => o.Id.Equals(newsItemId));

            newsItem.HasBeenViewed = isRead;
        }

        #endregion
    }
}
