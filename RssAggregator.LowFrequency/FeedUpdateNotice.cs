using System;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedUpdateNotice
    {
        public DateTime RefreshTime { get; set; }
        public string FeedUri { get; set; }
    }
}