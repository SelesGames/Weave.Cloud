using Common.Azure.SmartBlobClient;
using Ninject;
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
                    ContentType = "application/protobuf", 
                    //UseGzipOnUpload = true,
                };

            //var azureCred = new AzureCredentials(
            //    "weaveuser",
            //    "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==",
            //    false);

            var userRepo = new UserRepository(blobClient);
            Bind<UserRepository>().ToConstant(userRepo).InSingletonScope();
        }
    }
}
