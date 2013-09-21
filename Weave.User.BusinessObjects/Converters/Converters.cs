using SelesGames.Common;
using System.Linq;


namespace Weave.User.BusinessObjects.Converters
{
    public class Converters :
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>,
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.Image, Image>
    {
        public static readonly Converters Instance = new Converters();

        public Image Convert(RssAggregator.Core.DTOs.Outgoing.Image o)
        {
            return new Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        public NewsItem Convert(RssAggregator.Core.DTOs.Outgoing.NewsItem o)
        {
            return new NewsItem
            {
                Id = o.Id,
                //FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                UtcPublishDateTimeString = o.PublishDateTime,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                HasBeenViewed = false,
                IsFavorite = false,
                Image = o.Image == null ? null : o.Image.Convert<RssAggregator.Core.DTOs.Outgoing.Image, Image>(Instance),
            };
        }
    }
}
