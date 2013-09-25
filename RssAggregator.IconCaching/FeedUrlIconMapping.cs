using System.Collections.Generic;

namespace RssAggregator.IconCaching
{
    public class FeedUrlIconMappings : List<FeedUrlIconMapping>
    { }

    public class FeedUrlIconMapping
    {
        public string Url { get; set; }
        public string IconUrl { get; set; }
    }
}
