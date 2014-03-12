using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using System;
using Weave.RssAggregator.HighFrequency;

namespace RssAggregator.Role.HighFrequency
{
    internal class StartupTask
    {
        IKernel kernel;
        HighFrequencyFeedUpdater hfUpdater;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            
            SetHighFrequencyValues();

            hfUpdater.InitializeAsync().Wait();
#if DEBUG
            hfUpdater.RefreshAllFeedsImmediately().Wait();
#endif
            hfUpdater.StartFeedRefreshTimer();
        }

        void SetHighFrequencyValues()
        {
            string feedLibraryUrl;
            TimeSpan highFrequencyRefreshPeriod;

            string temp;

            temp = RoleEnvironment.GetConfigurationSettingValue("HighFrequencyRefreshPeriod");
            highFrequencyRefreshPeriod = TimeSpan.FromMinutes(double.Parse(temp));

            temp = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");
            feedLibraryUrl = temp;

            hfUpdater = new HighFrequencyFeedUpdater(
                feedLibraryUrl, 
                kernel.Get<SequentialProcessor>(), 
                highFrequencyRefreshPeriod);

            kernel.Bind<HighFrequencyFeedUpdater>().ToMethod(_ => hfUpdater).InSingletonScope();
        }
    }
}