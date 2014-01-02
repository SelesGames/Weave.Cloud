using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Ninject;

namespace Weave.User.Paging.AzureCloudService.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var userCred = new StorageCredentials("weaveuser2", "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==");
            var userCsa = new CloudStorageAccount(userCred, false);

            var pagingCred = new StorageCredentials("weaveuser2", "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==");
            var pagingCsa = new CloudStorageAccount(pagingCred, false);

            //var updater = new UserPagedNewsUpdater()


            //this.Bind<CloudStorageAccount>().ToConstant(csa).InSingletonScope();
        }
    }
}