using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.LowFrequency
{
    public class CachedFeed
    {
        public Guid FeedId { get; private set; }
        public string Name { get; private set; }
        public string FeedUri { get; private set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; set; }
        //public IReadOnlyList<NewsItem> News { get; set; }
        public FeedState LastFeedState { get; set; }


        public enum FeedState
        {
            Uninitialized,
            Failed,
            OK
        }

        public CachedFeed(string name, string feedUri)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name in HighFrequencyFeed ctor");
            if (string.IsNullOrWhiteSpace(feedUri)) throw new ArgumentException("name in HighFrequencyFeed ctor");

            Name = name;
            FeedUri = feedUri;
            InitializeId();
            LastFeedState = FeedState.OK;
        }

        void InitializeId()
        {
            FeedId = CryptoHelper.ComputeHashUsedByMobilizer(FeedUri);
        }

        public override string ToString()
        {
            return Name + ": " + FeedUri;
        }
    }
}