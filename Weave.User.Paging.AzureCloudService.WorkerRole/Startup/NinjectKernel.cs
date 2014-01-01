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

            var credentials = new StorageCredentials("weaveuser2", "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==");
            var csa = new CloudStorageAccount(credentials, false);
            this.Bind<CloudStorageAccount>().ToConstant(csa).InSingletonScope();
        }
    }
}