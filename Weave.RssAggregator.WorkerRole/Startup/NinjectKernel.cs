using Common.Caching;
using Ninject;
using RssAggregator.IconCaching;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

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