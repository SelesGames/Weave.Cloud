﻿using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class UserInfo
    {
        List<Feed> feedsList = new List<Feed>();

        public Guid Id { get; set; }
        public IReadOnlyList<Feed> Feeds { get { return feedsList; } }
        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }


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

        public void AddFeed(Feed feed, bool trustSource = false)
        {
            if (feed == null) return;
            if (feedsList == null) feedsList = new List<Feed>();

            // if we don't trust the Feed was created correctly, verify it's Id and that no existing Feed matches
            if (!trustSource)
            {
                feed.EnsureGuidIsSet();

                // if any existing feed has a matching Id, don't add it
                if (feedsList.Any(o => o.Id.Equals(feed.Id)))
                    return;
            }

            feedsList.Add(feed);
        }

        public void RemoveFeed(Guid feedId)
        {
            if (feedsList == null || !feedsList.Any())
                return;

            var matching = feedsList.FirstOrDefault(o => o.Id.Equals(feedId));
            if (matching != null)
            {
                feedsList.Remove(matching);
            }
        }

        public void UpdateFeed(Feed feed)
        {
            if (feedsList == null || !feedsList.Any() || feed == null)
                return;

            feed.EnsureGuidIsSet();

            var matching = feedsList.FirstOrDefault(o => o.Id.Equals(feed.Id));
            if (matching != null)
            {
                // the only 3 fields the user can change are category, feed name, and article viewing type
                matching.Category = feed.Category;
                matching.FeedName = feed.FeedName;
                matching.ArticleViewingType = feed.ArticleViewingType;
            }
        }

        public async Task MarkNewsItemRead(Guid feedId, Guid newsItemId)
        {
            var newsItem = FindNewsItem(feedId, newsItemId);
            if (newsItem == null)
                return;

            var saved = newsItem.Convert<NewsItem, Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem>(Converters.Instance);
            await ArticleServiceClient.Current.MarkRead(Id, saved);
            newsItem.HasBeenViewed = true;
        }

        public async Task MarkNewsItemUnread(Guid feedId, Guid newsItemId)
        {
            var newsItem = FindNewsItem(feedId, newsItemId);
            if (newsItem == null)
                return;

            await ArticleServiceClient.Current.RemoveRead(Id, newsItemId);
            newsItem.HasBeenViewed = false;
        }




        #region helper methods

        async Task RefreshFeeds(IEnumerable<Feed> feeds)
        {
            if (feeds == null || !feeds.Any())
                return;

            var client = new NewsServer();
            foreach (var feed in feeds)
                feed.RefreshNews(client);

            client.SendRequests();
            await Task.WhenAll(feeds.Select(o => o.CurrentRefresh));
        }

        NewsItem FindNewsItem(Guid feedId, Guid newsItemId)
        {
            if (feedsList == null || !feedsList.Any())
                return null;

            var newsItem = feedsList
                .Where(o => o.Id.Equals(feedId))
                .SelectMany(o => o.News)
                .FirstOrDefault(o => o.Id.Equals(newsItemId));

            return newsItem;
        }

        #endregion
    }
}
