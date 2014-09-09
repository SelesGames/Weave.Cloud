using System;

namespace Weave.User.BusinessObjects.Mutable
{
    public class UserIndex
    {        
        public Guid Id { get; set; }
        public FeedIndices FeedIndices { get; private set; }
        public Articles Articles { get; private set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public ArticleDeleteTime ArticleDeletionTimeForMarkedRead { get; set; }
        public ArticleDeleteTime ArticleDeletionTimeForUnread { get; set; }

        public UserIndex()
        {
            FeedIndices = new FeedIndices();
            Articles = new Articles(this);
        }
    }
}