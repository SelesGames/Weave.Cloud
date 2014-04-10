using SelesGames.Common;
using System.Linq;
using Weave.User.BusinessObjects.v2;

namespace Weave.User.Service.WorkerRole.v2.Converters
{
    public class Converters : IConverter<ExtendedNewsItem, Weave.Article.Service.DTOs.ServerIncoming.SavedNewsItem>
    {
        public static readonly Converters Instance = new Converters();

        public Article.Service.DTOs.ServerIncoming.SavedNewsItem Convert(ExtendedNewsItem ni)
        {
            var o = ni.NewsItem;

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
                SourceName = ni.Feed.Name,
                Tags = ni.Feed.Category == null ? null : new[] { ni.Feed.Category }.ToList(),
                //Image = o.Image == null ? null : o.Image.Convert<Image, Outgoing.Image>(Instance),
            };
        }
    }
}
