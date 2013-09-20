using RssAggregator.Client.Converters;
using SelesGames.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Client;
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

        public async Task<FeedResult> GetNewsAsync(FeedRequest request)
        {
            FeedResult result;
            try
            {
                var requester = CreateRequester(request, RequestTimeout);
                var requestStatus = await requester.UpdateFeed();

                if (requestStatus == Feed.RequestStatus.Unmodified)
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
                        News = requester.News.Select(o => o.Convert(EntryToNewsItemConverter.Instance)).ToList(),
                    };
                }
            }
            catch
            {
                result = new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
            }
            return result;
        }

        static Feed CreateRequester(FeedRequest request, TimeSpan timeout)
        {
            return new Feed
            {
                FeedUri = request.Url,
                MostRecentNewsItemPubDate = request.MostRecentNewsItemPubDate,
                Etag = request.Etag,
                LastModified = request.LastModified,
                UpdateTimeOut = timeout,
            };
        }
    }
}