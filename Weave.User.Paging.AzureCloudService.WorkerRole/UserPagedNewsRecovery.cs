using Common.Azure.SmartBlobClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Paging.Lists;

namespace Weave.User.Paging.AzureCloudService.WorkerRole
{
    public class UserPagedNewsRecovery
    {
        #region Private member variables

        readonly Guid userId;
        readonly SmartBlobClient userClient;
        readonly SmartBlobClient pageClient;
        readonly string containerName;

        readonly string MASTER_LIST = "masterlist";
        readonly string USER_CONTAINER = "user";
        readonly string USER_APPEND = ".user";

        #endregion




        public async Task<List<Store.News.NewsItem>> GetNewsForFeeds(
            Guid userId, 
            Guid feedId, 
            int skip, 
            int take)
        {
            var masterList = await GetMasterListsInfo();
            var latestList = masterList.FeedLists
                .Where(o => o.FeedId == feedId)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefault();

            var pageSize = latestList.PageSize;

            var startPage = skip / pageSize;
            var endPage = (skip + take) / pageSize;
            var numberOfPagesToTake = endPage - startPage + 1;

            var pages = latestList.PagedNews
                .Skip(startPage)
                .Take(numberOfPagesToTake)
                .ToList();

            var recoveredStoredPages = await Task.WhenAll(pages
                .Select(o => GetPage(o.CreateFileName())));

            return recoveredStoredPages
                .OrderBy(o => o.Index)
                .SelectMany(o => o.News)
                .ToList();
        }

        public async Task<List<Store.News.NewsItem>> GetNewsForFeeds(
            Guid userId,
            Guid feedId,
            Guid listId,
            int skip,
            int take)
        {
            var masterList = await GetMasterListsInfo();
            var latestList = masterList.FeedLists
                .Where(o => o.FeedId == feedId && o.ListId == listId)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefault();

            var pageSize = latestList.PageSize;

            var startPage = skip / pageSize;
            var endPage = (skip + take) / pageSize;
            var numberOfPagesToTake = endPage - startPage + 1;

            var pages = latestList.PagedNews
                .Skip(startPage)
                .Take(numberOfPagesToTake)
                .ToList();

            var recoveredStoredPages = await Task.WhenAll(pages
                .Select(o => GetPage(o.CreateFileName())));

            return recoveredStoredPages
                .OrderBy(o => o.Index)
                .SelectMany(o => o.News)
                .ToList();
        }

        Task<Store.News.PagedNews> GetPage(string blobName)
        {
            return pageClient.Get<Store.News.PagedNews>(containerName, blobName);
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
    }
}
