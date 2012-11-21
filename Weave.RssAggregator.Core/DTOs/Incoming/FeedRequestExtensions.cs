using System;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.DTOs.Incoming
{
    public static class FeedRequestExtensions
    {
        public async static Task<FeedResult> GetNewsAsync(this FeedRequest request, TimeSpan timeout)
        {
            FeedResult result;
            try
            {
                var requester = CreateRequester(request, timeout);
                var requestStatus = await requester.UpdateFeed().ConfigureAwait(false);

                if (requestStatus == FeedRequester.RequestStatus.Unmodified)
                {
                    result = new FeedResult { Id = request.Id, Status = FeedResultStatus.Unmodified };
                }
                else
                {
                    result = new FeedResult
                    {
                        Id = request.Id,
                        Status = FeedResultStatus.OK,
                        Etag = requester.Etag,
                        LastModified = requester.LastModified,
                        MostRecentNewsItemPubDate = requester.MostRecentNewsItemPubDate,
                        OldestNewsItemPubDate = requester.OldestNewsItemPubDate,
                        News = requester.News,
                    };
                }
            }
            catch
            {
                result = new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
            }
            return result;
        }

        static FeedRequester CreateRequester(FeedRequest request, TimeSpan timeout)
        {
            return new FeedRequester
            {
                FeedUri = request.Url,
                MostRecentNewsItemPubDate = request.MostRecentNewsItemPubDate,
                Etag = request.Etag,
                LastModified = request.LastModified,
                TimeOut = timeout,
            };
        }
    }
}