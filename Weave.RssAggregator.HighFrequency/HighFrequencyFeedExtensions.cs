﻿using System;
using System.Linq;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.Core.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public static class HighFrequencyFeedExtensions
    {
        public static FeedResult ToFeedResult(this HighFrequencyFeed feed, FeedRequest request)
        {
            try
            {
                if (feed.LastFeedState == HighFrequencyFeed.FeedState.Failed)
                {
                    return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
                }
                else
                {
                    var newsWithDate = feed.News
                        .Select(o => new { date = RssHelperFunctions.TryGetUtcDate(o.PublishDateTime), newsItem = o })
                        .Where(o => o.date.Item1)
                        .Select(o => new { date = o.date.Item2, newsItem = o.newsItem });


                    var filteredNews = newsWithDate;

                    var previousMostRecentNewsItemPubDateString = request.MostRecentNewsItemPubDate;

                    var tryGetPreviousMostRecentDate = RssHelperFunctions.TryGetUtcDate(previousMostRecentNewsItemPubDateString);
                    if (tryGetPreviousMostRecentDate.Item1)
                    {
                        var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
                        filteredNews = newsWithDate.TakeWhile(o => o.date > previousMostRecentNewsItemPubDate);
                    }

                    var newNewsList = filteredNews.Select(o => Copy(o.newsItem)).ToList();

                    if (newNewsList.Count == 0)
                    {
                        return new FeedResult { Id = request.Id, Status = FeedResultStatus.Unmodified };
                    }
                    else
                    {
                        // REMOVE THE DESCRIPTION IF IT IS SUPPRESSED ON THIS FEED
                        if (feed.IsDescriptionSuppressed)
                        {
                            foreach (var newsItem in newNewsList)
                                newsItem.Description = null;
                        }

                        var result = new FeedResult
                        {
                            Id = request.Id,
                            MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
                            OldestNewsItemPubDate = feed.OldestNewsItemPubDate,
                            Etag = feed.Etag,
                            LastModified = feed.LastModified,
                            News = newNewsList,
                            Status = FeedResultStatus.OK,
                        };
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
            }
        }

        static NewsItem Copy(NewsItem o)
        {
            return new NewsItem
            {
                Title = o.Title,
                PublishDateTime = o.PublishDateTime,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                Description = o.Description,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
            };
        }
    }
}
