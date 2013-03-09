using System;
using System.Collections.Generic;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedUpdateDto
    {
        public Guid FeedId { get; set; }
        public string Name { get; set; }
        public string FeedUri { get; set; }
        public DateTime RefreshTime { get; set; }
        public List<EntryWithPostProcessInfo> Entries { get; set; }
    }
}
