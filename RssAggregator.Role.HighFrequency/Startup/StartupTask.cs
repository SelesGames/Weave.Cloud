using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using StackExchange.Redis;
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
            Common.Compression.Settings.CompressionHandlers = new Common.Compression.Windows.CompressionHandlerCollection();

            kernel = new NinjectKernel();
            
            SetHighFrequencyValues();

            try
            {
                hfUpdater.InitializeAsync().Wait();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
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
                kernel.Get<ConnectionMultiplexer>());
            hfUpdater.RefreshPeriod = highFrequencyRefreshPeriod;

            kernel.Bind<HighFrequencyFeedUpdater>().ToMethod(_ => hfUpdater).InSingletonScope();
        }
    }
}