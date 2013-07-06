using Common.Caching;
using SelesGames.Common;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Service.Converters;
using Weave.User.Service.Repositories;
using Store = Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class TempUserCache : IBasicCache<Guid, Task<UserInfo>>
    {
        IExtendedCache<Guid, Task<UserInfo>> innerCache;
        UserRepository userRepo;

        public TempUserCache(UserRepository userRepo, IExtendedCache<Guid, Task<UserInfo>> innerCache)
        {
            this.userRepo = userRepo;
            this.innerCache = innerCache;
        }

        public Task<UserInfo> Get(Guid key)
        {
            return innerCache.GetOrAdd(key, GetFromRepository);
        }

        async Task<UserInfo> GetFromRepository(Guid key)
        {
            var user = await userRepo.Get(key);
            var userBO = user.Convert<Store.UserInfo, UserInfo>(DataStoreToBusinessObject.Instance);
            return userBO;
        }
    }
}
