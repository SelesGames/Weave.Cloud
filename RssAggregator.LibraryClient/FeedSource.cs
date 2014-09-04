
namespace Weave.RssAggregator.LibraryClient
{
    public class FeedSource
    {
        public string FeedName { get; set; }
        public string FeedUri { get; set; }
        public string CorrectedUri { get; set; }
        public string IconUrl { get; set; }
        public string Category { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
        public string Instructions { get; set; }
    }
}