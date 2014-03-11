using Ninject;
using Weave.Mobilizer.Cache;
using Weave.Mobilizer.Cache.Readability;

namespace Weave.Mobilizer.WorkerRole.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            var azureClient = new AzureClient("weave", "uudFrra70qkI64bifaI2Rrm37CZ1HkzaBQrLMyw6U/hmzNDZehXeo9DdUv7BCpuZY4N2q/CNpNwYxW2fa218xA==");

            Bind<AzureClient>().ToConstant(azureClient).InSingletonScope();

            var azureCache = new AzureStorageCache(azureClient);
            var localCache = new LocalMemoryCache();
            var rc = new ReadabilityClient("a142c0cc575c6760d5c46247f8aa6aabbacb6fd8");
            var selector = new MobilizerSelector(rc);
            var nLevelCache = new MobilizerResultCache(selector, localCache, azureCache);

            Bind<AzureStorageCache>().ToConstant(azureCache).InSingletonScope();
            Bind<LocalMemoryCache>().ToConstant(localCache).InSingletonScope();
            Bind<MobilizerResultCache>().ToConstant(nLevelCache).InSingletonScope();
        }
    }
}
