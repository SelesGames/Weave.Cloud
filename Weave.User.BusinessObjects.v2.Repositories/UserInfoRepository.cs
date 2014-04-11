using Common.Azure.SmartBlobClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Converters = Weave.User.Service.Converters.v2;
using Store = Weave.User.DataStore.v2;

namespace Weave.User.BusinessObjects.v2.Repositories
{
    public class UserInfoRepository
    {
        string account, key;

        const string USER_CONTAINER = "user";
        const string NEWS_CONTAINER = "news";
        const string NEWS_STATE_CONTAINER = "newsState";


        public UserInfoRepository(string account, string key)
        {
            this.account = account;
            this.key = key;
        }




        #region Get

        public async Task<UserInfo> GetUser(Guid id)
        {
            var key = id.ToString("N");
            var client = CreateUserClient();
            var user = await client.Get<Store.UserInfo>(USER_CONTAINER, key).ConfigureAwait(false);
            var converted = Convert(user);
            return converted;
        }

        public async Task<MasterNewsItemCollection> GetAllNews(Guid id)
        {
            var key = id.ToString("N");
            var client = CreateNewsClient();
            var news = await client.Get<Store.MasterNewsItemCollection>(NEWS_CONTAINER, key).ConfigureAwait(false);
            var converted = Convert(news);
            return converted;
        }

        public async Task<NewsItemStateCache> GetNewsItemStateCache(Guid id)
        {
            var key = id.ToString("N");
            var client = CreateNewsStateClient();
            var newsState = await client.Get<Store.NewsItemStateCache>(NEWS_STATE_CONTAINER, key).ConfigureAwait(false);
            var converted = Convert(newsState);
            converted.UserId = id;
            return converted;
        }

        #endregion




        #region Save

        public Task Save(UserInfo user)
        {
            var key = user.Id.ToString("N");
            var store = Convert(user);
            var client = CreateUserClient();
            return client.Save(USER_CONTAINER, key, store);
        }

        public Task Save(MasterNewsItemCollection allNews)
        {
            var key = allNews.UserId.ToString("N");
            var store = Convert(allNews);
            var client = CreateNewsClient();
            return client.Save(NEWS_CONTAINER, key, store);
        }

        public Task Save(NewsItemStateCache newsState)
        {
            var key = newsState.UserId.ToString("N");
            var store = Convert(newsState);
            var client = CreateNewsStateClient();
            return client.Save(NEWS_STATE_CONTAINER, key, store);
        }

        #endregion




        #region Conversion helpers

        UserInfo Convert(Store.UserInfo user)
        {
 	        return Converters.DataStoreToBusinessObject.Instance.Convert(user);
        }

        MasterNewsItemCollection Convert(Store.MasterNewsItemCollection news)
        {
            var allNews = news.News == null ? null : news.News.Select(Convert);

            return new MasterNewsItemCollection(allNews)
            {
                UserId = news.UserId,
            };
        }

        NewsItem Convert(Store.NewsItem newsItem)
        {
            return Converters.DataStoreToBusinessObject.Instance.Convert(newsItem);
        }

        NewsItemStateCache Convert(Store.NewsItemStateCache newsState)
        {
            var cache = new NewsItemStateCache(newsState);
            return cache;
        }

        Store.UserInfo Convert(UserInfo user)
        {
            return Converters.BusinessObjectToDataStore.Instance.Convert(user);
        }

        Store.MasterNewsItemCollection Convert(MasterNewsItemCollection allNews)
        {
            var news = allNews.Values.SelectMany(o => o).Select(Convert).ToList();

            return new Store.MasterNewsItemCollection
            {
                UserId = allNews.UserId,
                News = news,
            };
        }

        Store.NewsItem Convert(NewsItem newsItem)
        {
            return Converters.BusinessObjectToDataStore.Instance.Convert(newsItem);
        }

        Store.NewsItemStateCache Convert(NewsItemStateCache newsState)
        {
            return newsState.Inner;
        }

        #endregion




        #region SmartBlobClient creation helpers

        SmartBlobClient CreateUserClient()
        {
 	        var client = new SmartBlobClient(account, key, false);
            client.UseCompressionByDefault = false;
            client.DefaultContentType = "application/json";
            return client;
        }

        SmartBlobClient CreateNewsClient()
        {
 	        var client = new SmartBlobClient(account, key, false);
            client.UseCompressionByDefault = true;
            client.DefaultContentType = "application/json";
            return client;
        }

        SmartBlobClient CreateNewsStateClient()
        {
 	        var client = new SmartBlobClient(account, key, false);
            client.UseCompressionByDefault = false;
            client.DefaultContentType = "application/json";
            return client;
        }

        #endregion
    }
}