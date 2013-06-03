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
        public IReadOnlyList<string> Instructions { get; set; }
        public IReadOnlyList<EntryWithPostProcessInfo> Entries { get; set; }
    }
}
