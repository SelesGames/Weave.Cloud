﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.DTOs;

namespace Weave.RssAggregator.HighFrequency
{
    /// <summary>
    /// Will add the full canonical info for the articles to the news items cache,
    /// and will also add the news item Ids/Score tuple to the sorted news item
    /// index for a particular feed
    /// </summary>
    public class RedisArticleCacheProcessor : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        NewsItemCacheAdder adder;

        public RedisArticleCacheProcessor(NewsItemCacheAdder adder)
        {
            this.adder = adder;
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            try
            {
                var entries = update.Entries ?? new List<EntryWithPostProcessInfo>();
                var news = entries.Select(Map).ToList();

                var feedId = update.FeedId;

                await adder.AddNews(feedId, news);
            }
            catch { }
        }

        NewsItem Map(EntryWithPostProcessInfo o)
        {
            return new NewsItem
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                UtcPublishDateTimeString = o.UtcPublishDateTimeString,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.OriginalImageUrl,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Image = !o.ShouldIncludeImage ? null : new Image
                {
                    Width = o.ImageWidth,
                    Height = o.ImageHeight,
                    BaseImageUrl = o.BaseResizedImageUrl,
                    OriginalUrl = o.OriginalImageUrl,
                    SupportedFormats = o.SupportedFormats,
                },
            };
        }
    }
}