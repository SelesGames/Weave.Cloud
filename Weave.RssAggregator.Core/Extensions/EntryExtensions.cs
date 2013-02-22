using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    public static class EntryExtensions
    {
        public static NewsItem AsNewsItem(this Entry e)
        {
            return new NewsItem
            {
                Title = e.Title,
                Link = e.Link,
                ImageUrl = e.ImageUrl,
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
