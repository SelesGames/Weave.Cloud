using Common.Caching;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.User.Service.Cache
{
    public class LocalUserCache : LocalMemoryCache<Guid, Task<UserInfo>>
    {
        IUserWriter writer;

        public LocalUserCache(IUserWriter writer)
        {
            this.writer = writer;
        }

        public override async void OnExpire(Task<UserInfo> obj)
        {
            var user = await obj;
            await writer.ImmediateWrite(user);
        }
    }
}
