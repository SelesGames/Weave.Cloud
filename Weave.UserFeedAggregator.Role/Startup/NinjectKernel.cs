using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
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

            var cred = new StorageCredentials(
                "weaveuser",
                "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==");

            var csa = new CloudStorageAccount(cred, useHttps: false);

            var userInfoBlobClient = new UserInfoBlobClient(csa, containerName: "user");
            var azureDataCacheClient = new UserInfoAzureCacheClient(userInfoBlobClient);
            var userRepo = new UserRepository(azureDataCacheClient);
            Bind<UserRepository>().ToConstant(userRepo).InSingletonScope();

            Bind<IWeaveArticleService>().To<Article.Service.Client.ServiceClient>().InSingletonScope();
        }
    }
}
