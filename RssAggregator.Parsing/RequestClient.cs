using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Parsing;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.DTOs.Incoming
{
    public class RequestClient
    {
        public TimeSpan RequestTimeout { get; set; }

        public RequestClient()
        {
            RequestTimeout = TimeSpan.FromSeconds(30);
        }

        public async Task<Result> GetNewsAsync(Request request)
        {
            Result result;
            try
            {
                var feed = new Feed
                {
                    FeedUri = request.Url,
                    MostRecentNewsItemPubDate = request.MostRecentNewsItemPubDate,
                    Etag = request.Etag,
                    LastModified = request.LastModified,
                    UpdateTimeOut = RequestTimeout,
                };

                var requestStatus = await feed.Update();

                if (requestStatus == Feed.RequestStatus.Unmodified)
                {
                    result = new Result { Status = FeedResultStatus.Unmodified };
                }
                else
                {
                    result = new Result
                    {
                        Status = FeedResultStatus.OK,
                        Etag = feed.Etag,
                        LastModified = feed.LastModified,
                        MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
                        OldestNewsItemPubDate = feed.OldestNewsItemPubDate,
                    };
                }
            }
            catch
            {
                result = new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
            }
            return result;
        }
    }
}