using System;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public interface IArticleQueueService
    {
        void QueueMarkRead(Guid userId, Guid newsItemId, string source);
        void QueueMarkUnread(Guid userId, Guid newsItemId, string source);
        void QueueAddFavorite(Guid userId, Guid newsItemId, string source);
        void QueueRemoveFavorite(Guid userId, Guid newsItemId, string source);
    }
}
