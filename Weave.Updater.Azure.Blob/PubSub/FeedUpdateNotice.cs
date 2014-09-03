using System;

namespace Weave.Updater.PubSub
{
    public class FeedUpdateNotice
    {
        public DateTime RefreshTime { get; set; }
        public string FeedUri { get; set; }
    }
}