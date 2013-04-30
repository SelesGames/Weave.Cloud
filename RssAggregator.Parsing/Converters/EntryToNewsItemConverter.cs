using SelesGames.Common;
using Weave.Parsing;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace RssAggregator.Client.Converters
{
    internal class EntryToNewsItemConverter : IConverter<Entry, NewsItem>
    {
        public static readonly EntryToNewsItemConverter Instance = new EntryToNewsItemConverter();

        public NewsItem Convert(Entry e)
        {
            return new NewsItem
            {
                Title = e.Title,
                Link = e.Link,
                ImageUrl = e.GetImageUrl(),
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
    }
}
