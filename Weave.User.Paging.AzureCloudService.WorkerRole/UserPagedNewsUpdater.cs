using Common.Azure.SmartBlobClient;
using Microsoft.WindowsAzure.Storage;
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

        readonly int PAGE_SIZE = 50;
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

            // create the container for this User if it doesn't already exist
            var container = pageClient.GetContainerHandle(containerName);
            await container.CreateIfNotExistsAsync();


            await user.RefreshAllFeeds();
            var updateLists = PagedNewsHelper.CalculatePagedNews(user, PAGE_SIZE);

            var masterList = await GetMasterListsInfo();
            masterList.AllNewsLists.Add(updateLists.UpdatedAllNewsList);
            masterList.CategoryLists.AddRange(updateLists.UpdatedCategoryLists);
            masterList.FeedLists.AddRange(updateLists.UpdatedFeedLists);

            var pagedNews = new IEnumerable<PagedNewsBase>[] 
            {
                updateLists.UpdatedAllNewsList.PagedNews, 
                updateLists.UpdatedCategoryLists.SelectMany(o => o.PagedNews), 
                updateLists.UpdatedFeedLists.SelectMany(o => o.PagedNews)
            }
            .SelectMany(o => o);

            await Task.WhenAll(pagedNews.Select(Save));
            await Save(masterList);
        }




        #region Save functions

        async Task Save(PagedNewsBase pagedNews)
        {
            var fileName = pagedNews.CreateFileName();
            var store = pagedNews.CreateSerializablePagedNews();

            await pageClient.Save(containerName, fileName, store);
        }

        async Task Save(MasterListsInfo masterList)
        {
            var store = new Weave.User.Paging.Store.Lists.MasterListsInfo
            {
                UserId = masterList.UserId,
            };
            await pageClient.Save(containerName, MASTER_LIST, masterList);
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
                return new MasterListsInfo();
            }
        }

        #endregion
    }
}
