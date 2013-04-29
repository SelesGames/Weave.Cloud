using SelesGames.WebApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Incoming = Weave.Article.Service.DTOs.ServerIncoming;
using Outgoing = Weave.Article.Service.DTOs.ServerOutgoing;

namespace Weave.Article.Service.WorkerRole.Controllers
{
    public class ArticleController : ApiController
    {
        SqlClient client;

        public ArticleController(SqlClient client)
        {
            this.client = client;
        }

        [HttpPost]
        [ActionName("mark_read")]
        public async Task<bool> MarkRead(Guid userId, [FromBody] Incoming.SavedNewsItem newsItem)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItem == null) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Missing NewsItem object in message body");

            var o = Convert(newsItem);
            o.Id = Guid.NewGuid();
            o.AddedOn = DateTime.UtcNow;

            var result = await client.MarkNewsItemRead(userId, o);
            return result;
        }

        [HttpGet]
        [ActionName("remove_read")]
        public Task RemoveRead(Guid userId, Guid newsItemId)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItemId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid newsItemId");

            return client.RemoveNewsItemRead(userId, newsItemId);
        }

        [HttpGet]
        [ActionName("get_read")]
        public Task<List<Outgoing.SavedNewsItem>> GetRead(Guid userId, int take, int skip = 0)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");

            return client.GetNewsItemRead(userId, take, skip);
        }

        [HttpPost]
        [ActionName("add_favorite")]
        public async Task<bool> AddFavorite(Guid userId, [FromBody] Incoming.SavedNewsItem newsItem)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItem == null) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Missing NewsItem object in message body");

            var o = Convert(newsItem);
            o.Id = Guid.NewGuid();
            o.AddedOn = DateTime.UtcNow;

            var result = await client.AddNewsItemFavorite(userId, o);
            return result;
        }

        [HttpGet]
        [ActionName("remove_favorite")]
        public Task RemoveFavorite(Guid userId, Guid newsItemId)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItemId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid newsItemId");

            return client.RemoveNewsItemFavorite(userId, newsItemId);
        }

        [HttpGet]
        [ActionName("get_favorites")]
        public Task<List<Outgoing.SavedNewsItem>> GetFavorites(Guid userId, int take, int skip = 0)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");

            return client.GetNewsItemFavorites(userId, take, skip);
        }


        #region helper functions

        Outgoing.SavedNewsItem Convert(Incoming.SavedNewsItem o)
        {
            return new Outgoing.SavedNewsItem
            {
                SourceName = o.SourceName,
                Title = o.Title,
                Link = o.Link,
                UtcPublishDateTime = o.UtcPublishDateTime,
                ImageUrl = o.ImageUrl,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Notes = o.Notes,
                Tags = o.Tags,
                Image = o.Image,
            };
        }

        #endregion
    }
}
