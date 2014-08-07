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

        public async Task<FeedResult> GetNewsAsync(FeedRequest request)
        {
            FeedResult result;
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
                    result = new FeedResult { Id = request.Id, Status = FeedResultStatus.Unmodified };
                }
                else
                {
                    result = new FeedResult
                    {
                        Id = request.Id,
                        Status = FeedResultStatus.OK,
                        Etag = feed.Etag,
                        LastModified = feed.LastModified,
                        MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
                        OldestNewsItemPubDate = feed.OldestNewsItemPubDate,
                        News = feed.News.Select(Map).ToList(),
                    };
                }
            }
            catch
            {
                result = new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
            }
            return result;
        }




        #region Map functions

        static NewsItem Map(Entry e)
        {
            return new NewsItem
            {
                Title = e.Title,
                Link = e.Link,
                ImageUrl = e.ImageUrls.FirstOrDefault(),
                PublishDateTime = e.UtcPublishDateTimeString,
                Description = null,//entry.Description,
                VideoUri = e.VideoUri,
                YoutubeId = e.YoutubeId,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                Id = e.Id,
                FeedId = e.FeedId,
            };
        }

        #endregion
    }
}