using Common.Azure.SmartBlobClient;
using Common.Caching;
using Ninject;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.UserFeedAggregator.Cache;
using Weave.UserFeedAggregator.Repositories;

namespace Weave.UserFeedAggregator.Role.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var blobClient = new SmartBlobClient(
                storageAccountName: "weaveuser",
                key: "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==",
                container: "user",
                useHttps: false)
                {
                    ContentType = "application/json", 
                    //UseGzipOnUpload = true,
                };

            var userRepo = new UserRepository(blobClient);
            Bind<UserRepository>().ToConstant(userRepo).InSingletonScope();
            Bind<IUserWriter>().To<TempUserWriter>().InSingletonScope();
            Bind<IExtendedCache<Guid, Task<UserInfo>>>().To<LocalUserCache>().InSingletonScope();
            Bind<IBasicCache<Guid, Task<UserInfo>>>().To<TempUserCache>().InSingletonScope();
        }
    }
}
