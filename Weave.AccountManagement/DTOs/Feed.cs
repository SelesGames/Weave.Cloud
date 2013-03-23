using System;

namespace Weave.AccountManagement.DTOs
{
    public class Feed
    {
        public Guid Id { get; set; }
        public string FeedName { get; set; }
        public string FeedUri { get; set; }
        public string Category { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
    }
}
