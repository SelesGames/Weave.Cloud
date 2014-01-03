using System;

namespace Weave.User.Paging.BusinessObjects.News
{
    public class PagedNewsByFeed : PagedNewsBase
    {
        public Guid FeedId { get; set; }

        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}-{2}-{3}",
                UserId.ToString("N"),
                FeedId.ToString("N"),
                ListId.ToString("N"),
                Index);
        }
    }
}
