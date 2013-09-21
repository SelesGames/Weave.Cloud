using SelesGames.Common;
using System.Linq;
using Weave.User.BusinessObjects;


namespace Weave.User.Service.Role.Converters
{
    public class Converters : IConverter<NewsItem, Weave.Article.Service.DTOs.ServerIncoming.SavedNewsItem>
    {
        public static readonly Converters Instance = new Converters();

        public Article.Service.DTOs.ServerIncoming.SavedNewsItem Convert(NewsItem o)
        {
            return new Article.Service.DTOs.ServerIncoming.SavedNewsItem
            {
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl, 
                UtcPublishDateTime = o.UtcPublishDateTimeString,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                SourceName = o.Feed.Name,
                Tags = o.Feed.Category == null ? null : new[] { o.Feed.Category }.ToList(),
                //Image = o.Image == null ? null : o.Image.Convert<Image, Outgoing.Image>(Instance),
            };
        }
    }
}
