using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Weave.RssAggregator.LowFrequency;

namespace RssAggregator.WebRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        FeedCache hfCache;

        public void OnStart()
        {
            kernel = NinjectKernel.Current;
            
            SetLowFrequencyValues();
            SetHighFrequencyValues();

            hfCache.InitializeAsync().Wait();
        }

        void SetLowFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("LowFrequencyHttpWebRequestTimeout");
            var value = int.Parse(temp);
            AppSettings.SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(value);
        }

        void SetHighFrequencyValues()
        {
            var feedLibraryUrl = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");

            hfCache = new FeedCache(
                feedLibraryUrl, 
                kernel.Get<DbClient>());

            kernel.Bind<FeedCache>().ToMethod(_ => hfCache).InSingletonScope();
        }
    }
}
