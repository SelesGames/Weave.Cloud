using Common.Azure.SmartBlobClient;
using Ninject;
using Weave.Article.Service.Contracts;
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

            Bind<IWeaveArticleService>().To<Article.Service.Client.ServiceClient>().InSingletonScope();
        }
    }
}
