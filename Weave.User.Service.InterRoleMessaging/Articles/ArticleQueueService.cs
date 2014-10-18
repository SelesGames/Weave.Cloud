using StackExchange.Redis;
using System;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public class ArticleQueueService : IArticleQueueService
    {
        readonly ArticleStateChangeMessageQueue innerQueue;

        public ArticleQueueService()
        {
            this.innerQueue = new ArticleStateChangeMessageQueue();
        }

        public void QueueMarkRead(Guid userId, Guid newsItemId, string source)
        {
            Validate(userId, newsItemId);

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
            Validate(userId, newsItemId);

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
            Validate(userId, newsItemId);

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
            Validate(userId, newsItemId);

            innerQueue.Push(
                new ArticleStateChangeNotification
                {
                    UserId = userId,
                    ArticleId = newsItemId,
                    Change = ArticleStateChange.Unfavorite,
                    Source = source,
                });
        }

        void Validate(Guid userId, Guid newsItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("userId should not be empty");

            if (newsItemId == Guid.Empty)
                throw new ArgumentException("newsItemId should not be empty");
        }
    }
}
