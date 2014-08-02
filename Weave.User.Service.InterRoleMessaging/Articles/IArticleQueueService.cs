using System;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public interface IArticleQueueService
    {
        void QueueMarkRead(Guid userId, Guid newsItemId);
        void QueueMarkUnread(Guid userId, Guid newsItemId);
        void QueueAddFavorite(Guid userId, Guid newsItemId);
        void QueueRemoveFavorite(Guid userId, Guid newsItemId);
    }
}
