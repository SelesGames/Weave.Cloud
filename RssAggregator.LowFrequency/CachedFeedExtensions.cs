using Common.TimeFormatting;
using System;
using System.Collections.Generic;
using System.Linq;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.LowFrequency
{
    public static class CachedFeedExtensions
    {
        public static IEnumerable<T> TakeSince<T>(this IEnumerable<T> source, Func<T, DateTime> criteria, DateTime cutoff)
        {
            return source.TakeWhile(o => criteria(o) > cutoff);
        }

        public static FeedResult ToFeedResult(this CachedFeed feed, FeedRequest request)
        {
            try
            {
                if (feed.LastFeedState == CachedFeed.FeedState.Failed || feed.LastFeedState == CachedFeed.FeedState.Uninitialized)
                {
                    return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed, FromCache = true };
                }
                else
                {
                    var newsWithDate = feed.News
                        .Select(o => new { date = o.PublishDateTime.TryGetUtcDate(), newsItem = o })
                        .Where(o => o.date.Item1)
                        .Select(o => new { date = o.date.Item2, newsItem = o.newsItem });


                    var filteredNews = newsWithDate;

                    var previousMostRecentNewsItemPubDateString = request.MostRecentNewsItemPubDate;

                    if (!string.IsNullOrWhiteSpace(previousMostRecentNewsItemPubDateString))
                    {

                        var tryGetPreviousMostRecentDate = previousMostRecentNewsItemPubDateString.TryGetUtcDate();
                        if (tryGetPreviousMostRecentDate.Item1)
                        {
                            var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
                            filteredNews = newsWithDate.TakeWhile(o => o.date > previousMostRecentNewsItemPubDate);
                        }
                    }

                    var newNewsList = filteredNews.Select(o => o.newsItem).ToList();

                    if (newNewsList.Count == 0)
                    {
                        return new FeedResult { Id = request.Id, Status = FeedResultStatus.Unmodified, FromCache = true };
                    }
                    else
                    {
                        var result = new FeedResult
                        {
                            Status = FeedResultStatus.OK,
                            Id = request.Id,
                            MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
                            OldestNewsItemPubDate = feed.OldestNewsItemPubDate,
                            News = newNewsList,
                            FromCache = true,
                        };
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed, FromCache = true };
            }
        }
    }
}


//static NewsItem Copy(NewsItem o)
//{
//    return new NewsItem
//    {
//        Title = o.Title,
//        PublishDateTime = o.PublishDateTime,
//        Link = o.Link,
//        ImageUrl = o.ImageUrl,
//        Description = o.Description,
//        YoutubeId = o.YoutubeId,
//        VideoUri = o.VideoUri,
//        PodcastUri = o.PodcastUri,
//        ZuneAppId = o.ZuneAppId,
//        Id = o.Id,
//        FeedId = o.FeedId,
//    };
//}