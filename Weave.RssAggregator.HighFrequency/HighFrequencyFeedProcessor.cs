using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedProcessor : ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, FeedRequester>>
    {
        public bool IsHandledFully { get; private set; }

        public Task ProcessAsync(Tuple<HighFrequencyFeed, FeedRequester> tup)
        {
            var feed = tup.Item1;
            var requester = tup.Item2;

            try
            {
                var result = requester.Status;

                if (result == FeedRequester.RequestStatus.OK)
                {
                    feed.Etag = requester.Etag;
                    feed.LastModified = requester.LastModified;
                    feed.MostRecentNewsItemPubDate = requester.MostRecentNewsItemPubDate;
                    feed.OldestNewsItemPubDate = requester.OldestNewsItemPubDate;
                    feed.News = requester.News.Select(o => o.AsNewsItem()).ToList();

                    DebugEx.WriteLine("REFRESHED {0}  ({1})", feed.Name, feed.FeedUri);
                }
                else if (result == FeedRequester.RequestStatus.Unmodified)
                {
                    DebugEx.WriteLine("UNMODIFIED {0}  ({1})", feed.Name, feed.FeedUri);
                }
                feed.LastFeedState = HighFrequencyFeed.FeedState.OK;
            }
            catch (TaskCanceledException ex)
            {
                DebugEx.WriteLine("!!!!!! TIMED OUT {0}  ({1}): {2}", feed.Name, feed.FeedUri, ex.Message);
                feed.LastFeedState = HighFrequencyFeed.FeedState.Failed;
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null)
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}: {3}", feed.Name, feed.FeedUri, ex.Message, ex.InnerException.Message);
                else
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", feed.Name, feed.FeedUri, ex.Message);

                feed.LastFeedState = HighFrequencyFeed.FeedState.Failed;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", feed.Name, feed.FeedUri, ex.Message);
                feed.LastFeedState = HighFrequencyFeed.FeedState.Failed;
            }

            return Task.FromResult<object>(null);
        }
    }
}
