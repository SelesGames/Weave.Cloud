using SelesGames.HttpClient;
using System;
using System.Threading;
using System.Threading.Tasks;
using Weave.Article.Service.DTOs;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.UserFeedAggregator
{
    public class ArticleServiceClient
    {
        public static readonly ArticleServiceClient Current = new ArticleServiceClient();

        const string SERVICE_URL = "http://weave-article.cloudapp.net/api/article";
        readonly SmartHttpClient client = CreateHttpClient();


        public Task MarkRead(Guid userId, NewsItem newsItem)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Not a valid userId");
            if (newsItem == null) throw new ArgumentNullException("newsItem can't be null");

            var url = string.Format("{0}/mark_read?userId={1}", SERVICE_URL, userId);
            return client.PostAsync(url, newsItem, CancellationToken.None);
        }

        public Task RemoveRead(Guid userId, Guid newsItemId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Not a valid userId");
            if (newsItemId == Guid.Empty) throw new ArgumentException("Not a valid newsItemId");

            var url = string.Format("{0}/remove_read?userId={1}&newsItemId={2}", SERVICE_URL, userId, newsItemId);
            return client.GetAsync(url, CancellationToken.None);
        }

        public Task AddFavorite(Guid userId, FavoriteNewsItem newsItem)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Not a valid userId");
            if (newsItem == null) throw new ArgumentNullException("Missing NewsItem object in message body");

            var url = string.Format("{0}/add_favorite?userId={1}", SERVICE_URL, userId);
            return client.PostAsync(url, newsItem, CancellationToken.None);
        }




        #region Helper methods

        static SmartHttpClient CreateHttpClient()
        {
            return new SmartHttpClient(ContentEncoderSettings.Json, CompressionSettings.None);
        }

        #endregion
    }
}
