﻿using SelesGames.Common;
using Weave.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryToEntryWithPostProcessInfoConverter : IConverter<Entry, EntryWithPostProcessInfo>
    {
        public static readonly EntryToEntryWithPostProcessInfoConverter Instance = new EntryToEntryWithPostProcessInfoConverter();

        public EntryWithPostProcessInfo Convert(Entry e)
        {
            return new EntryWithPostProcessInfo
            {
                Id = e.Id,
                FeedId = e.FeedId,
                UtcPublishDateTime = e.UtcPublishDateTime,
                Title = e.Title,
                OriginalPublishDateTimeString = e.OriginalPublishDateTimeString,
                Link = e.Link,
                OriginalImageUrl = e.GetImageUrl(),
                Description = e.Description,
                YoutubeId = e.YoutubeId,
                VideoUri = e.VideoUri,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                OriginalRssXml = e.OriginalPublishDateTimeString,
            };
        }
    }
}
