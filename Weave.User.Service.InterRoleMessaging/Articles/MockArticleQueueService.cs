using System;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public class MockArticleQueueService : IArticleQueueService
    {
        public void QueueMarkRead(Guid userId, Guid newsItemId, string source)
        {
        }

        public void QueueMarkUnread(Guid userId, Guid newsItemId, string source)
        {
        }

        public void QueueAddFavorite(Guid userId, Guid newsItemId, string source)
        {
        }

        public void QueueRemoveFavorite(Guid userId, Guid newsItemId, string source)
        {
        }
    }
}
