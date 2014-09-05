using Ninject;
using StackExchange.Redis;
using Weave.User.Service.InterRoleMessaging.Articles;

namespace Weave.User.Service.Role.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var redisClientConfig = ConfigurationOptions.Parse(
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=");

            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);

            Bind<ConnectionMultiplexer>().ToConstant(connectionMultiplexer).InSingletonScope();
            Bind<IArticleQueueService>().To<ArticleQueueService>();
        }
    }
}