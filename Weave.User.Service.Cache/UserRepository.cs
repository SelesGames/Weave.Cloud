using SelesGames.Common;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Service.Converters;
using Store = Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class UserRepository
    {
        UserInfoBlobWriteQueue writeQueue;
        UserInfoBlobClient userInfoBlobClient;

        public UserRepository(UserInfoBlobClient userInfoBlobClient)
        {
            this.userInfoBlobClient = userInfoBlobClient;
            writeQueue = new UserInfoBlobWriteQueue(userInfoBlobClient);
        }

        public async Task<UserInfo> Get(Guid userId)
        {
            var store = await userInfoBlobClient.Get(userId);
            var user = store.Convert<Store.UserInfo, UserInfo>(DataStoreToBusinessObject.Instance);
            return user;
        }

        public void Save(Guid userId, UserInfo user)
        {
            var store = user.Convert<UserInfo, Store.UserInfo>(BusinessObjectToDataStore.Instance);
            writeQueue.Add(store);
        }
    }
}
