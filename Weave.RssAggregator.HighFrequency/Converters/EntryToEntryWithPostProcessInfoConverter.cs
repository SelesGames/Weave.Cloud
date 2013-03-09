using SelesGames.Common;
using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryToEntryWithPostProcessInfoConverter : IConverter<Entry, EntryWithPostProcessInfo>
    {
        public static EntryToEntryWithPostProcessInfoConverter Instance = new EntryToEntryWithPostProcessInfoConverter();

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
                OriginalImageUrl = e.ImageUrl,
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
