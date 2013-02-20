using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Web.Http.Dependencies;
using Weave.RssAggregator.LowFrequency;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        FeedCache hfCache;
        IDependencyResolver resolver;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel); 
            
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
            string feedLibraryUrl;
            int highFrequencyRefreshSplit;
            TimeSpan highFrequencyRefreshPeriod;

            string temp;

            temp = RoleEnvironment.GetConfigurationSettingValue("HighFrequencyRefreshPeriod");
            highFrequencyRefreshPeriod = TimeSpan.FromMinutes(double.Parse(temp));

            temp = RoleEnvironment.GetConfigurationSettingValue("HighFrequencyRefreshSplit");
            highFrequencyRefreshSplit = int.Parse(temp);

            temp = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");
            feedLibraryUrl = temp;

            hfCache = new FeedCache(
                feedLibraryUrl, 
                kernel.Get<DbClient>(), 
                highFrequencyRefreshSplit, 
                highFrequencyRefreshPeriod);

            kernel.Bind<FeedCache>().ToMethod(_ => hfCache).InSingletonScope();
        }
    }
}
