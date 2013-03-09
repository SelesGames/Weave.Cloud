using SelesGames.Common;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryWithPostProcessInfoToNewsItemConverter : IConverter<EntryWithPostProcessInfo, NewsItem>
    {
        public static EntryWithPostProcessInfoToNewsItemConverter Instance = new EntryWithPostProcessInfoToNewsItemConverter();

        public NewsItem Convert(EntryWithPostProcessInfo e)
        {
            return new Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem
            {
                Title = e.Title,
                Link = e.Link,
                ImageUrl = e.PreferredImageUrl,
                PublishDateTime = e.UtcPublishDateTimeString,
                Description = null,
                VideoUri = e.VideoUri,
                YoutubeId = e.YoutubeId,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                Id = e.Id,
                FeedId = e.FeedId,
                Image = !e.IsResizedImageSet ? null : new Core.DTOs.Outgoing.Image
                {
                    Width = e.ImageWidth,
                    Height = e.ImageHeight,
                    BaseImageUrl = e.BaseResizedImageUrl,
                    OriginalUrl = e.OriginalImageUrl,
                    SupportedFormats = e.SupportedFormats,
                }
            };
        }
    }
}
