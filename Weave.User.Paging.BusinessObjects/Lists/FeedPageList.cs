using System;

namespace Weave.User.Paging.BusinessObjects.Lists
{
    public class FeedPageList : BasePageList
    {
        public Guid FeedId { get; set; }

        public override string CreateFileName()
        {
            return string.Format(
                "{0}.PageList",
                FeedId.ToString("N"));
        }
    }
}
