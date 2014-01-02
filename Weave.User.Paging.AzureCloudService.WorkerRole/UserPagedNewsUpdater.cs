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
using Weave.User.Paging.Lists;
using Weave.User.Paging.News;

namespace Weave.User.Paging
{
    public class UserPagedNewsUpdater
    {
        #region Private member variables

        readonly Guid userId;
        readonly SmartBlobClient userClient;
        readonly SmartBlobClient pageClient;
        readonly string containerName;

        readonly int PAGE_SIZE = 100;
        readonly string MASTER_LIST = "masterlist";
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
            this.containerName = userId.ToString("N");
        }

        #endregion




        public async Task UpdateUsersPagedLists()
        {
            // get the user.  If no use matches this id, throw an exception
            var user = await GetUser();
            await user.RefreshAllFeeds();

            // create the container for this User if it doesn't already exist
            var container = pageClient.GetContainerHandle(containerName);
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, null, null);

            var masterList = await GetMasterListsInfo();

            var updateLists = PagedNewsHelper.CalculatePagedNewsSince(user, masterList, PAGE_SIZE);

            masterList.AllNewsLists.Add(updateLists.UpdatedAllNewsList);
            masterList.CategoryLists.AddRange(updateLists.UpdatedCategoryLists);
            masterList.FeedLists.AddRange(updateLists.UpdatedFeedLists);

            var pagedNews = new IEnumerable<PagedNewsBase>[] 
            {
                updateLists.UpdatedAllNewsList.PagedNews, 
                updateLists.UpdatedCategoryLists.SelectMany(o => o.PagedNews), 
                updateLists.UpdatedFeedLists.SelectMany(o => o.PagedNews)
            }
            .OfType<IEnumerable<PagedNewsBase>>()
            .SelectMany(o => o);

            await Task.WhenAll(pagedNews.Select(Save));
            await Save(masterList);
            await Save(user);
        }




        #region Save functions

        async Task Save(PagedNewsBase pagedNews)
        {
            var fileName = pagedNews.CreateFileName();
            var store = pagedNews.CreateSerializablePagedNews();
            await pageClient.Save(containerName, fileName, store,
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

        async Task Save(MasterListsInfo masterList)
        {
            var store = Convert(masterList);
            await pageClient.Save(containerName, MASTER_LIST, store);
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




        #region Get functions

        async Task<UserInfo> GetUser()
        {
            var blobName = string.Format("{0}{1}", userId, USER_APPEND);
            var storedUser = await userClient.Get<DataStore.UserInfo>(USER_CONTAINER, blobName);
            var converter = Service.Converters.DataStoreToBusinessObject.Instance;
            return storedUser.Convert<DataStore.UserInfo, UserInfo>(converter);
        }

        async Task<MasterListsInfo> GetMasterListsInfo()
        {
            try
            {
                var masterListsInfo = await pageClient.Get<MasterListsInfo>(containerName, MASTER_LIST);
                return masterListsInfo;
            }
            catch
            {
                return new MasterListsInfo { UserId = userId };
            }
        }

        #endregion




        #region Conversion helpers

        Store.Lists.MasterListsInfo Convert(MasterListsInfo o)
        {
            return new Store.Lists.MasterListsInfo
            {
                UserId = o.UserId,
                AllNewsLists = o.AllNewsLists == null ? null :
                    o.AllNewsLists.Select(Convert).ToList(),
                CategoryLists = o.CategoryLists == null ? null :
                    o.CategoryLists.Select(Convert).ToList(),
                FeedLists = o.FeedLists == null ? null :
                    o.FeedLists.Select(Convert).ToList(),
            };
        }

        Store.Lists.ListInfoByAll Convert(ListInfoByAll o)
        {
            return new Store.Lists.ListInfoByAll
            {
                ListId = o.ListId,
                CreatedOn = o.CreatedOn,
                LastAccess = o.LastAccess,
                PageSize = o.PageSize,
                PageCount = o.PageCount,
            };
        }

        Store.Lists.ListInfoByCategory Convert(ListInfoByCategory o)
        {
            return new Store.Lists.ListInfoByCategory
            {
                ListId = o.ListId,
                CreatedOn = o.CreatedOn,
                LastAccess = o.LastAccess,
                PageSize = o.PageSize,
                PageCount = o.PageCount,
                Category = o.Category,
            };
        }

        Store.Lists.ListInfoByFeed Convert(ListInfoByFeed o)
        {
            return new Store.Lists.ListInfoByFeed
            {
                ListId = o.ListId,
                CreatedOn = o.CreatedOn,
                LastAccess = o.LastAccess,
                PageSize = o.PageSize,
                PageCount = o.PageCount,
                FeedId = o.FeedId,
            };
        }

        #endregion
    }
}