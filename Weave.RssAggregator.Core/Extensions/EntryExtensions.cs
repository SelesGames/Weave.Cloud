using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    public static class EntryExtensions
    {
        public static NewsItem AsNewsItem(this Entry entry)
        {
            return new NewsItem
            {
                Title = entry.Title,
                PublishDateTime = entry.PublishDateTimeString,
                Link = entry.Link,
                ImageUrl = entry.ImageUrl,
                Description = null,//entry.Description,
                YoutubeId = entry.YoutubeId,
                VideoUri = entry.VideoUri,
                PodcastUri = entry.PodcastUri,
                ZuneAppId = entry.ZuneAppId,
            };
        }
    }
}
