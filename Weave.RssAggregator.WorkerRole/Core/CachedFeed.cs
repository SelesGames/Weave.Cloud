using System;

namespace Weave.RssAggregator
{
    public class CachedFeed
    {
        public string Name { get; private set; }
        public string Uri { get; private set; }

        public CachedFeed(string name, string feedUri)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name in CachedFeed ctor");
            if (string.IsNullOrWhiteSpace(feedUri)) throw new ArgumentException("name in CachedFeed ctor");

            Name = name;
            Uri = feedUri;
        }

        public override string ToString()
        {
            return Name + ": " + Uri;
        }
    }
}