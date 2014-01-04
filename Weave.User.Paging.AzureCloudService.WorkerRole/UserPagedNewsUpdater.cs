using Common.Azure.SmartBlobClient;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Paging.BusinessObjects;
using Weave.User.Paging.BusinessObjects.Lists;
using Weave.User.Paging.BusinessObjects.News;

namespace Weave.User.Paging
{
    public class UserPagedNewsUpdater
    {
        #region Private member variables

        readonly Guid userId;
        readonly SmartBlobClient userClient;
        readonly SmartBlobClient pageClient;
        readonly string userContainer;

        readonly int PAGE_SIZE = 100;
        readonly string USER_CONTAINER = "user";
        readonly string USER_APPEND = ".user";

        #endregion




        #region Constructor

        public UserPagedNewsUpdater(
            Guid userId,
            CloudStorageAccount userStorageAccount, 
            CloudStorageAccount pageStorageAccount)
        {
            this.userId = userId;
            this.userClient = new SmartBlobClient(userStorageAccount);
            this.pageClient = new SmartBlobClient(pageStorageAccount);
            this.userContainer = userId.ToString("N");
        }

        #endregion




        public async Task UpdateUsersPagedLists()
        {
            // get the user.  If no use matches this id, throw an exception
            var user = await GetUser();
            await user.RefreshAllFeeds();

            // create the container for this User if it doesn't already exist
            var container = pageClient.GetContainerHandle(userContainer);
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, null, null);

            var masterList = await CreatePageListCollection(user.Feeds);

            var helper = PagedNewsHelper.CalculatePagedNewsSince(user, masterList, PAGE_SIZE);
            var updatedLists = helper.UpdatedLists;

            var updatedPageLists = new List<BasePageList>();
            foreach (var list in updatedLists.OfType<CategoryListInfo>())
            {
                var pageList = masterList.Get(list.Category);
                if (pageList != null)
                {
                    pageList.Add(list);
                    updatedPageLists.Add(pageList);
                }
            }

            foreach (var list in updatedLists.OfType<FeedListInfo>())
                masterList.Add(list);

            var pagedNews = updatedLists.SelectMany(o => o.PagedNews);

            await Task.WhenAll(pagedNews.Select(Save));
            await Write(updatedPageLists);
            await Save(user);
        }




        #region Get functions

        async Task<UserInfo> GetUser()
        {
            var blobName = string.Format("{0}{1}", userId, USER_APPEND);
            var storedUser = await userClient.Get<DataStore.UserInfo>(USER_CONTAINER, blobName);
            var converter = Service.Converters.DataStoreToBusinessObject.Instance;
            return storedUser.Convert<DataStore.UserInfo, UserInfo>(converter);
        }

        async Task<PageListCollection> CreatePageListCollection(IEnumerable<Feed> feeds)
        {
            var categoriesTask = feeds.GetFullCategoryList().Select(GetPageList);
            var categoryLists = await Task.WhenAll(categoriesTask);

            var feedsTask = feeds.Select(o => o.Id).Select(GetPageList);
            var feedLists = await Task.WhenAll(feedsTask);

            return new PageListCollection(categoryLists, feedLists);
        }

        async Task<CategoryPageList> GetPageList(string category)
        {
            var fileName = new CategoryPageList { Category = category }.CreateFileName();
            try
            {
                var temp = await pageClient.Get<Store.Lists.CategoryPageList>(userContainer, fileName);
                return Convert(temp);
            }
            catch { }
            return new CategoryPageList { Category = category };
        }

        async Task<FeedPageList> GetPageList(Guid feedId)
        {
            var fileName = new FeedPageList { FeedId = feedId }.CreateFileName();
            try
            {
                var temp = await pageClient.Get<Store.Lists.FeedPageList>(userContainer, fileName);
                return Convert(temp);
            }
            catch { }
            return new FeedPageList { FeedId = feedId };
        }

        #endregion




        #region Save functions

        async Task Save(PagedNewsBase pagedNews)
        {
            var fileName = pagedNews.CreateFileName();
            var store = pagedNews.CreateSerializablePagedNews();
            await pageClient.Save(userContainer, fileName, store,
                options: new Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 10),
                    MaximumExecutionTime = TimeSpan.FromMinutes(10),
                    ServerTimeout = TimeSpan.FromMinutes(10),
                },
                blobProperties: new BlobProperties
                {
                    ContentEncoding = "gzip",
                    ContentType = "application/json"
                });
        }

        async Task Write(IEnumerable<BasePageList> pageLists)
        {
            var tasks = pageLists.OfType<CategoryPageList>().Select(Save)
                .Union(pageLists.OfType<FeedPageList>().Select(Save));

            await Task.WhenAll(tasks);
        }

        async Task Save(CategoryPageList list)
        {
            var fileName = list.CreateFileName();
            var store = Convert(list);
            await pageClient.Save(userContainer, fileName, store,
                options: new Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 10),
                    MaximumExecutionTime = TimeSpan.FromMinutes(1),
                },
                blobProperties: new BlobProperties
                {
                    ContentEncoding = "gzip",
                    ContentType = "application/json"
                });
        }

        async Task Save(FeedPageList list)
        {
            var fileName = list.CreateFileName();
            var store = Convert(list);
            await pageClient.Save(userContainer, fileName, store,
                options: new Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions
                {
                    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 10),
                    MaximumExecutionTime = TimeSpan.FromMinutes(1),
                },
                blobProperties: new BlobProperties
                {
                    ContentEncoding = "gzip",
                    ContentType = "application/json"
                });
        }

        async Task Save(UserInfo user)
        {
            var blobName = string.Format("{0}{1}", userId, USER_APPEND);
            var converter = Service.Converters.BusinessObjectToDataStore.Instance;
            var storedUser = user.Convert<UserInfo, DataStore.UserInfo>(converter);
            await userClient.Save(USER_CONTAINER, blobName, storedUser,
                new Common.Azure.Blob.WriteRequestProperties
                {
                    ContentType = "application/json",
                    UseCompression = true,
                });
        }

        #endregion




        #region Conversion helpers

        CategoryPageList Convert(Store.Lists.CategoryPageList o)
        {
            return new CategoryPageList
            {
                Category = o.Category,
                LatestListId = o.LatestListId,
                LatestRefresh = o.LatestRefresh,
                Lists = o.Lists == null ? null : o.Lists.Select(Convert).ToList(),
            };
        }

        FeedPageList Convert(Store.Lists.FeedPageList o)
        {
            return new FeedPageList
            {
                FeedId = o.FeedId,
                LatestListId = o.LatestListId,
                LatestRefresh = o.LatestRefresh,
                Lists = o.Lists == null ? null : o.Lists.Select(Convert).ToList(),
            };
        }

        ListInfo Convert(Store.Lists.ListInfo o)
        {
            return new ListInfo
            {
                ListId = o.ListId,
                CreatedOn = o.CreatedOn,
                LastAccess = o.LastAccess,
                PageSize = o.PageSize,
                PageCount = o.PageCount,
            };
        }

        Store.Lists.CategoryPageList Convert(CategoryPageList o)
        {
            return new Store.Lists.CategoryPageList
            {
                Category = o.Category,
                LatestListId = o.LatestListId,
                LatestRefresh = o.LatestRefresh,
                Lists = o.Lists == null ? null : o.Lists.Select(Convert).ToList(),
            };
        }

        Store.Lists.FeedPageList Convert(FeedPageList o)
        {
            return new Store.Lists.FeedPageList
            {
                FeedId = o.FeedId,
                LatestListId = o.LatestListId,
                LatestRefresh = o.LatestRefresh,
                Lists = o.Lists == null ? null : o.Lists.Select(Convert).ToList(),
            };
        }

        Store.Lists.ListInfo Convert(ListInfo o)
        {
            return new Store.Lists.ListInfo
            {
                ListId = o.ListId,
                CreatedOn = o.CreatedOn,
                LastAccess = o.LastAccess,
                PageSize = o.PageSize,
                PageCount = o.PageCount,
            };
        }

        #endregion
    }
}