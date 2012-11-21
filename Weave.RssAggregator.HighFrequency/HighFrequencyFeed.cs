using System;
using System.Collections.Generic;
using Weave.RssAggregator.Core;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        public string Name { get; set; }
        public string FeedUri { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; set; }
        public List<NewsItem> News { get; set; }
        public FeedState LastFeedState { get; private set; }
        public bool IsDescriptionSuppressed { get; set; }

        public enum FeedState
        {
            Failed,
            OK
        }

        public HighFrequencyFeed()
        {
            News = new List<NewsItem>();
            LastFeedState = FeedState.Failed;
        }

        public async void Refresh()
        {
            try
            {
                var requester = new FeedRequester
                {
                    FeedUri = this.FeedUri,
                    Etag = this.Etag,
                    LastModified = this.LastModified,
                };
                var result = await requester.UpdateFeed();

                if (result == FeedRequester.RequestStatus.OK)
                {
                    this.Etag = requester.Etag;
                    this.LastModified = requester.LastModified;
                    this.MostRecentNewsItemPubDate = requester.MostRecentNewsItemPubDate;
                    this.OldestNewsItemPubDate = requester.OldestNewsItemPubDate;
                    this.News = requester.News;

                    DebugEx.WriteLine("REFRESHED {0}  ({1})", Name, FeedUri);
                }
                else if (result == FeedRequester.RequestStatus.Unmodified)
                {
                    DebugEx.WriteLine("UNMODIFIED {0}  ({1})", Name, FeedUri);
                }
                this.LastFeedState = FeedState.OK;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("FAILED {0}  ({1}): {2}", Name, FeedUri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
        }
    }
}
