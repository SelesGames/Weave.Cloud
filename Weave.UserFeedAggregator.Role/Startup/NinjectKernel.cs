using Common.Azure.SmartBlobClient;
using Common.Caching;
using Ninject;
using System;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Service.Cache;

namespace Weave.User.Service.Role.Startup
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
                    UseGzipOnUpload = true,
                };

            var userInfoBlobClient = new UserInfoBlobClient(blobClient);
            var azureDataCacheClient = new UserInfoAzureCacheClient(userInfoBlobClient);
            var userRepo = new UserRepository(azureDataCacheClient);
            Bind<UserRepository>().ToConstant(userRepo).InSingletonScope();
        }
    }
}
