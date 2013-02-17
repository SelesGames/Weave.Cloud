using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        Subject<List<Entry>> feedUpdate = new Subject<List<Entry>>();

        public Guid FeedId { get; set; }
        public string Name { get; set; }
        public string FeedUri { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; set; }
        public List<NewsItem> News { get; set; }
        public FeedState LastFeedState { get; set; }
        public bool IsDescriptionSuppressed { get; set; }

        public enum FeedState
        {
            Uninitialized,
            Failed,
            OK
        }

        public IObservable<List<Entry>> FeedUpdate { get; private set; }

        public HighFrequencyFeed()
        {
            News = new List<NewsItem>();
            LastFeedState = FeedState.Uninitialized;
            FeedUpdate = feedUpdate.AsObservable();
        }

        public void InitializeId()
        {
            FeedId = CryptoHelper.ComputeHashUsedByMobilizer(FeedUri);
        }

        public async Task Refresh()
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
                    this.News = requester.News.Select(o => o.AsNewsItem()).ToList();

                    feedUpdate.OnNext(requester.News);

                    DebugEx.WriteLine("REFRESHED {0}  ({1})", Name, FeedUri);
                }
                else if (result == FeedRequester.RequestStatus.Unmodified)
                {
                    DebugEx.WriteLine("UNMODIFIED {0}  ({1})", Name, FeedUri);
                }
                this.LastFeedState = FeedState.OK;
            }
            catch (TaskCanceledException ex)
            {
                DebugEx.WriteLine("!!!!!! TIMED OUT {0}  ({1}): {2}", Name, FeedUri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null)
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}: {3}", Name, FeedUri, ex.Message, ex.InnerException.Message);
                else
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, FeedUri, ex.Message);

                this.LastFeedState = FeedState.Failed;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, FeedUri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
        }

        public override string ToString()
        {
            return Name + ": " + FeedUri;
        }
    }
}
