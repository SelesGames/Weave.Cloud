using Common.Caching;
using Common.Data;
using Ninject;
using RssAggregator.IconCaching;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Weave.RssAggregator.WorkerRole.Startup
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

            var hfCache = new HighFrequencyFeedIconUrlCache();
            hfCache.BeginListeningToResourceChanges();

            var caches = new IExtendedCache<string, Task<string>>[] 
            {
                hfCache,
                new IconUrlAzureDataCache(),
                new DynamicIconUrlCache()
            };

            var nLevelCache = new NLevelIconUrlCache(caches);

            Bind<NLevelIconUrlCache>().ToConstant(nLevelCache).InSingletonScope();
        }
    }
}
