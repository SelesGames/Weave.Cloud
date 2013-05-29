using Microsoft.WindowsAzure;
using Ninject;
using Weave.Mobilizer.Core.Cache;
using Weave.Mobilizer.Core.Service;
using Weave.Readability;

namespace Weave.Mobilizer.NinjectKernel
{
    public class Kernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            ReadabilityClient rc = new ReadabilityClient("a142c0cc575c6760d5c46247f8aa6aabbacb6fd8");
            var azureClient = new AzureClient("weave", "uudFrra70qkI64bifaI2Rrm37CZ1HkzaBQrLMyw6U/hmzNDZehXeo9DdUv7BCpuZY4N2q/CNpNwYxW2fa218xA==");

            var azureCache = new AzureStorageCache(azureClient);
            var localCache = new LocalMemoryCache();
            var nLevelCache = new ReadabilityCache(rc, localCache, azureCache);

            Bind<AzureStorageCache>().ToConstant(azureCache).InSingletonScope();
            Bind<LocalMemoryCache>().ToConstant(localCache).InSingletonScope();
            Bind<ReadabilityCache>().ToConstant(nLevelCache).InSingletonScope();
        }
    }
}
