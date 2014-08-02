using System;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public class ArticleStateChangeNotification
    {
        public Guid UserId { get; set; }
        public Guid ArticleId { get; set; }
        public ArticleStateChange Change { get; set; }
    }
}
