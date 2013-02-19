using System;
using System.Collections.Generic;
using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedUpdateDto
    {
        public Guid FeedId { get; set; }
        public string Name { get; set; }
        public string FeedUri { get; set; }
        public List<Entry> Entries { get; set; }
    }
}
