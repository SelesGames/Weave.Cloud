using SelesGames.WebApi;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.Article.Service.WorkerRole.DTOs;
using Weave.RssAggregator.Core.DTOs.Outgoing;

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
        public async Task<bool> MarkRead(Guid userId, [FromBody] NewsItem newsItem)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItem == null) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Missing NewsItem object in message body");

            var result = await client.MarkNewsItemRead(userId, newsItem);
            return result;
        }

        [HttpPost]
        [ActionName("remove_read")]
        public async Task RemoveRead(Guid userId, Guid newsItemId)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItemId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid newsItemId");

            await client.RemoveNewsItemRead(userId, newsItemId);
        }

        [HttpPost]
        [ActionName("add_favorite")]
        public async Task<bool> AddFavorite(Guid userId, [FromBody] FavoriteNewsItem newsItem)
        {
            if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            if (newsItem == null) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Missing NewsItem object in message body");

            var result = await client.AddNewsItemFavorite(userId, newsItem);
            return result;
        }
    }
}
