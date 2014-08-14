using Common.Data;
using Ninject;
using SelesGames.Common;
using StackExchange.Redis;
using Weave.RssAggregator.HighFrequency;
using Weave.Updater.BusinessObjects;

namespace RssAggregator.Role.HighFrequency
{
    public class NinjectKernel : StandardKernel
    {
        #region connection strings

        const string SQL_CONN =
"Server=tcp:ykgd4qav8g.database.windows.net,1433;Database=weave;User ID=aemami99@ykgd4qav8g;Password=rzarecta99!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

        const string REDIS_CONN =
"weaveuser.redis.cache.windows.net,ssl=false,password=dM/xNBd9hB9Wgn3tPhkTsiwzIw4gImnS+eAN9sYuouY=";

        #endregion




        protected override void AddComponents()
        {
            base.AddComponents();

            Bind<SqlStoredProcClient>().ToMethod(_ => new SqlStoredProcClient(SQL_CONN));

            var redisClientConfig = ConfigurationOptions.Parse(REDIS_CONN);
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisClientConfig);
            Bind<ConnectionMultiplexer>().ToConstant(connectionMultiplexer).InSingletonScope();

            Bind<SequentialProcessor>().ToMethod(_ => new SequentialProcessor(
                new IProvider<ISequentialAsyncProcessor<FeedUpdate>>[]
                {
                    DelegateProvider.Create(() => this.Get<SqlSelectOnlyLatestNews>()),
                    DelegateProvider.Create(() => this.Get<RedirectResolver>()),
                    DelegateProvider.Create(() => this.Get<BestImageSelectorProcessor>()),
                    //DelegateProvider.Create(() => this.Get<ImageScalerUpdater>()),
                    DelegateProvider.Create(() => this.Get<EntryToBinaryUpdater>()),
                    DelegateProvider.Create(() => this.Get<SqlUpdater>()),
                    DelegateProvider.Create(() => this.Get<RedisFeedAndNewsProcessor>()),
                    DelegateProvider.Create(() => this.Get<MobilizerOverride>()),
                    DelegateProvider.Create(() => this.Get<PubSubUpdater>()),
                }))
                .InSingletonScope();
        }
    }
}