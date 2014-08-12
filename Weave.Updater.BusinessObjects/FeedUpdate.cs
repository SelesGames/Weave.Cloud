using System;
using System.Collections.Generic;

namespace Weave.Updater.BusinessObjects
{
    public class FeedUpdate
    {
        public Feed Feed { get; set; }
        public DateTime RefreshTime { get; set; }
        public IReadOnlyList<ExpandedEntry> Entries { get; set; }
    }
}