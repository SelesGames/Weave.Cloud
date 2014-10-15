using System;

namespace Weave.FeedUpdater.Messaging
{
    public class FeedUpdateNotice
    {
        public DateTime RefreshTime { get; set; }
        public string FeedUri { get; set; }
    }
}