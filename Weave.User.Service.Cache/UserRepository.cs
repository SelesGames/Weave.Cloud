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
        UserInfoAzureCacheClient cacheClient;

        public UserRepository(UserInfoAzureCacheClient cacheClient)
        {
            this.cacheClient = cacheClient;
        }

        public async Task<UserInfo> Get(Guid userId)
        {
            var cached = await cacheClient.Get(userId);
            var user = cached.Convert<Store.UserInfo, UserInfo>(DataStoreToBusinessObject.Instance);
            return user;
        }

        public void Save(Guid userId, UserInfo user)
        {
            var cached = user.Convert<UserInfo, Store.UserInfo>(BusinessObjectToDataStore.Instance);
            cacheClient.Update(userId, cached);
        }
    }
}
