using System;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedUpdateNotice
    {
        public Guid FeedId { get; set; }
        public DateTime RefreshTime { get; set; }
        public string FeedUri { get; set; }
    }
}