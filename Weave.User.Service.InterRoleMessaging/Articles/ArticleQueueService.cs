﻿using StackExchange.Redis;
using System;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public class ArticleQueueService : IArticleQueueService
    {
        readonly ArticleStateChangeMessageQueue innerQueue;

        public ArticleQueueService(ConnectionMultiplexer multiplexer)
        {
            this.innerQueue = new ArticleStateChangeMessageQueue(multiplexer);
        }

        public void QueueMarkRead(Guid userId, Guid newsItemId, string source)
        {
            innerQueue.Push(
                new ArticleStateChangeNotification
                {
                    UserId = userId,
                    ArticleId = newsItemId,
                    Change = ArticleStateChange.Read,
                    Source = source,
                });
        }

        public void QueueMarkUnread(Guid userId, Guid newsItemId, string source)
        {
            innerQueue.Push(
                new ArticleStateChangeNotification
                {
                    UserId = userId,
                    ArticleId = newsItemId,
                    Change = ArticleStateChange.Unread,
                    Source = source,
                });
        }

        public void QueueAddFavorite(Guid userId, Guid newsItemId, string source)
        {
            innerQueue.Push(
                new ArticleStateChangeNotification
                {
                    UserId = userId,
                    ArticleId = newsItemId,
                    Change = ArticleStateChange.Favorite,
                    Source = source,
                });
        }

        public void QueueRemoveFavorite(Guid userId, Guid newsItemId, string source)
        {
            innerQueue.Push(
                new ArticleStateChangeNotification
                {
                    UserId = userId,
                    ArticleId = newsItemId,
                    Change = ArticleStateChange.Unfavorite,
                    Source = source,
                });
        }
    }
}
