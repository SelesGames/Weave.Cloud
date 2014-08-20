using System;

namespace Weave.RssAggregator.LowFrequency
{
    public class CachedFeed
    {
        public string Name { get; private set; }
        public string Uri { get; private set; }
        //public string MostRecentNewsItemPubDate { get; set; }
        //public string OldestNewsItemPubDate { get; set; }
        //public FeedState LastFeedState { get; set; }


        //public enum FeedState
        //{
        //    Uninitialized,
        //    Failed,
        //    OK
        //}

        public CachedFeed(string name, string feedUri)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name in CachedFeed ctor");
            if (string.IsNullOrWhiteSpace(feedUri)) throw new ArgumentException("name in CachedFeed ctor");

            Name = name;
            Uri = feedUri;
            //LastFeedState = FeedState.OK;
        }

        public override string ToString()
        {
            return Name + ": " + Uri;
        }
    }
}