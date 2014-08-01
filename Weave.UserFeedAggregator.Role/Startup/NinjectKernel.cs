using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Ninject;
using StackExchange.Redis;
using Weave.User.Service.Cache;
using Weave.User.Service.Redis;
using Weave.User.Service.Role.Controllers;

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
            var userRepo = new UserRepository(userInfoBlobClient);
            Bind<UserRepository>().ToConstant(userRepo).InSingletonScope();

            var redisClientConfig = ConfigurationOptions.Parse(
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=");

            redisClientConfig.AllowAdmin = true;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);

            Bind<ConnectionMultiplexer>().ToConstant(connectionMultiplexer).InSingletonScope();

            var server = connectionMultiplexer.GetServer(
"weaveuser.redis.cache.windows.net", 6379);
            server.FlushDatabase(0);

            Bind<IArticleQueueService>().To<MockArticleQueueService>();
        }
    }
}
