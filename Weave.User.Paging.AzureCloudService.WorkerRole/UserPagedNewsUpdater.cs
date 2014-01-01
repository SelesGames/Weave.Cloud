using Common.Azure.SmartBlobClient;
using Microsoft.WindowsAzure.Storage;
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
        readonly UserInfo user;
        readonly SmartBlobClient client;
        readonly string containerName;

        readonly int PAGE_SIZE = 50;
        readonly string MASTER_LIST = "masterlist";

        public UserPagedNewsUpdater(UserInfo user, CloudStorageAccount csa)
        {
            this.user = user;
            this.client = new SmartBlobClient(csa);
            this.containerName = user.Id.ToString("N");
        }

        public async Task UpdateUsersPagedLists()
        {
            // create the container for this User if it doesn't already exist
            var container = client.GetContainerHandle(containerName);
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

            await client.Save(containerName, fileName, store);
        }

        async Task Save(MasterListsInfo masterList)
        {
            var store = new Weave.User.Paging.Store.Lists.MasterListsInfo
            {
                UserId = masterList.UserId,
            };
            await client.Save(containerName, MASTER_LIST, masterList);
        }

        #endregion




        #region Get functions

        async Task<MasterListsInfo> GetMasterListsInfo()
        {
            try
            {
                var masterListsInfo = await client.Get<MasterListsInfo>(containerName, MASTER_LIST);
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
