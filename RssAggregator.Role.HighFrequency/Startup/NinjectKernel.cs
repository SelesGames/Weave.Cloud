using Common.Azure.ServiceBus;
using Common.Data;
using Ninject;
using SelesGames.Common;
using StackExchange.Redis;
using Weave.RssAggregator.HighFrequency;
using Weave.User.Service.Redis;

namespace RssAggregator.Role.HighFrequency
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();


            var connectionString =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

            Bind<SqlStoredProcClient>().ToMethod(_ => new SqlStoredProcClient(connectionString));

            Bind<ServiceBusCredentials>().ToConstant(new ServiceBusCredentials
                {
                    Namespace = "weave-interop",
                    IssuerName = "owner",
                    IssuerKey = "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=",
                });

            Bind<TopicConnector>().ToMethod(_ => new TopicConnector(this.Get<ServiceBusCredentials>(), "FeedUpdatedTopic"))
                .WhenInjectedExactlyInto<ServiceBusUpdater>()
                .InSingletonScope();

            var redisClientConfig = ConfigurationOptions.Parse(
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=");

            //redisClientConfig.AllowAdmin = true;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);

            var newsItemCache = new NewsItemCache(connectionMultiplexer);
            var sortedNewsCache = new SortedNewsItemsSetCache(connectionMultiplexer);

            Bind<SequentialProcessor>().ToMethod(_ => new SequentialProcessor(
                new IProvider<ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>>[]
                {
                    DelegateProvider.Create(() => this.Get<SqlSelectOnlyLatestNews>()),
                    DelegateProvider.Create(() => this.Get<RedirectResolver>()),
                    DelegateProvider.Create(() => this.Get<BestImageSelectorProcessor>()),
                    DelegateProvider.Create(() => this.Get<ImageScalerUpdater>()),
                    DelegateProvider.Create(() => this.Get<EntryToBinaryUpdater>()),
                    DelegateProvider.Create(() => this.Get<SqlUpdater>()),
                    //DelegateProvider.Create(() => this.Get<RedisArticleCacheProcessor>()),
                    DelegateProvider.Create(() => this.Get<MobilizerOverride>()),
                    DelegateProvider.Create(() => this.Get<ServiceBusUpdater>()),
                }))
                .InSingletonScope();
        }
    }
}