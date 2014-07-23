﻿using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.Mutable.Extensions.Helpers
{
    static class UserMergeHelper
    {
        public static void Merge(UserInfo user, UserIndex userIndex)
        {
            if (user.Id != userIndex.Id)
                throw new Exception("Cannot merge user info that are not equivalent via their IDs!");

            userIndex.CopyTo(user);

            var diff = user.Feeds.Diff(userIndex.FeedIndices, o => o.Id, o => o.Id);

            foreach (var item in diff.Added)
            {
                var feed = new Feed();
                item.CopyTo(feed);
                user.Feeds.Add(feed);
            }

            foreach (var item in diff.Removed)
            {
                user.Feeds.RemoveWithId(item.Id);
            }

            foreach (var item in diff.Modified)
            {
                var feed = item.Item1;
                var feedIndex = item.Item2;

                Merge(feed, feedIndex);
            }
        }

        static void Merge(Feed feed, FeedIndex feedIndex)
        {
            feedIndex.CopyTo(feed);

            var diff = (feed.News ?? new List<NewsItem>())
                .Diff(feedIndex.NewsItemIndices, o => o.Id, o => o.Id);

            foreach (var item in diff.Modified)
            {
                var newsItem = item.Item1;
                var newsItemIndex = item.Item2;

                Merge(newsItem, newsItemIndex);
            }
        }

        static void Merge(NewsItem newsItem, NewsItemIndex newsItemIndex)
        {
            newsItemIndex.CopyTo(newsItem);
        }
    }
}