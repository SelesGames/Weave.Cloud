using System.Collections.Generic;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedUpdate
    {
        public HighFrequencyFeed Feed { get; private set; }
        public FeedUpdate InnerUpdate { get; private set; }

        public Feed InnerFeed { get { return InnerUpdate.Feed; } }
        public IEnumerable<ExpandedEntry> Entries { get { return InnerUpdate.Entries; } }

        internal HighFrequencyFeedUpdate(HighFrequencyFeed feed, FeedUpdate innerUpdate)
        {
            this.Feed = feed;
            this.InnerUpdate = innerUpdate;
        }
    }
}