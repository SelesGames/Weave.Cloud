using Common.Azure;
using SelesGames.Common;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Service.Converters;
using Store = Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class DataRepository
    {
        AzureDataCacheClient cacheClient;
        IAzureBlobClient blobClient;

        public DataRepository(IAzureBlobClient blobClient)
        {
            this.blobClient = blobClient;
            this.cacheClient = new AzureDataCacheClient(blobClient);
        }

        public async Task<UserInfo> GetMergedUser(Guid userId)
        {
            var userBlob = await blobClient.Get<Store.UserInfo>(userId);
            var user = userBlob.Convert<Store.UserInfo, UserInfo>(DataStoreToBusinessObject.Instance);
            var state = await GetUserNewsItemState(userId);
            state.SetNewsState(user);
            return user;
        }

        public async Task<UserNewsItemState> GetUserNewsItemState(Guid userId)
        {
            var state = await cacheClient.Get(userId);
            return state == null ? null : state.Convert<Store.UserNewsItemState, UserNewsItemState>(DataStoreToBusinessObject.Instance);
        }

        public void Save(Guid userId, UserNewsItemState userNewsState)
        {
            var state = userNewsState.Convert<UserNewsItemState, Store.UserNewsItemState>(BusinessObjectToDataStore.Instance);
            cacheClient.Update(userId, state);
        }
    }
}
