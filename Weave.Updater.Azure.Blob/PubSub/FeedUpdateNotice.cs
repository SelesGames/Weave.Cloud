using System;

namespace Weave.FeedUpdater.PubSub
{
    public class FeedUpdateNotice
    {
        public DateTime RefreshTime { get; set; }
        public string FeedUri { get; set; }
    }
}