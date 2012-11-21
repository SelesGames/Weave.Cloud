using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using System;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.Core
{
    public class Kernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            SetHighFrequencyValues();
            SetLowFrequencyValues();
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
            feedLibraryUrl = temp;// "http://weavestorage.blob.core.windows.net/settings/masterfeeds.xml";

            Bind<HighFrequencyFeedRssCache>()
                .ToConstant(new HighFrequencyFeedRssCache(feedLibraryUrl, highFrequencyRefreshSplit, highFrequencyRefreshPeriod))
                .InSingletonScope();
        }

        void SetLowFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("LowFrequencyHttpWebRequestTimeout");
            var value = int.Parse(temp);
            AppSettings.SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(value);
        }
    }
}
