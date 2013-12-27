using System;

namespace Weave.User.Paging
{
    public class PagedNewsByFeed : PagedNewsBase
    {
        public Guid FeedId { get; set; }

        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}-{2}",
                UserId.ToString("N"),
                FeedId.ToString("N"),
                Index);
        }
    }
}
