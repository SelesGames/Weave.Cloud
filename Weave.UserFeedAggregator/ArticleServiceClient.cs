using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.UserFeedAggregator
{
    public class ArticleServiceClient
    {
        const string SERVICE_URL = "http://weave-article.cloudapp.net/api/article";

        public Task MarkRead(Guid userId, NewsItem newsItem)
        {
            //if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            //if (newsItem == null) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Missing NewsItem object in message body");

            var url = string.Format("{0}?mark_read?userId={1}" + SERVICE_URL, userId);

            var client = new SmartHttpClient(ContentEncoderSettings.Protobuf, CompressionSettings.None);
            return client.PostAsync(url, newsItem, CancellationToken.None);
        }

        public Task RemoveRead(Guid userId, Guid newsItemId)
        {
            //if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
            //if (newsItemId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid newsItemId");

            var url = string.Format("{0}?remove_read?userId={1}&newsItemId={2}" + SERVICE_URL, userId, newsItemId);

            var client = new SmartHttpClient(ContentEncoderSettings.Protobuf, CompressionSettings.None);
            return client.GetAsync(url, CancellationToken.None);
        }

        //public async Task<bool> AddFavorite(Guid userId, FavoriteNewsItem newsItem)
        //{
        //    //if (userId == Guid.Empty) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Not a valid userId");
        //    //if (newsItem == null) throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "Missing NewsItem object in message body");

        //    var result = await client.AddNewsItemFavorite(userId, newsItem);
        //    return result;
        //}
    }
}
