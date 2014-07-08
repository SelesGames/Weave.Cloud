using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.v2
{
    public class UserArticleIndex
    {
        public UserArticleIndex()
        {
            Feeds = new FeedCollection();
        }
        
        public Guid Id { get; set; }
        public FeedCollection Feeds { get; private set; }

        public DateTime PreviousLoginTime { get; set; }
        public DateTime CurrentLoginTime { get; set; }

        public string ArticleDeletionTimeForMarkedRead { get; set; }
        public string ArticleDeletionTimeForUnread { get; set; }
    }
}
