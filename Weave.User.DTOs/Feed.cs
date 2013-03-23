using System;

namespace Weave.User.DTOs
{
    public class Feed
    {
        public Guid Id { get; set; }
        public string FeedName { get; set; }
        public string FeedUri { get; set; }
        public string Category { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public DateTime LastRefreshedOn { get; set; }
        //public Guid NewsHash { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
        //public List<UpdateParameters> UpdateHistory { get; set; }
    }
}
