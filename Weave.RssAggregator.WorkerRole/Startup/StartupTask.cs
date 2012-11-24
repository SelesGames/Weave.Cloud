﻿using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject.WebApi;
using System;
using System.Diagnostics;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.LowFrequency.Startup
{
    internal class StartupTask
    {
        HighFrequencyFeedCache hsfCache;

        public void OnStart()
        {
            SetLowFrequencyValues();
            SetHighFrequencyValues();
            CreateAndStartServer();
        }

        void CreateAndStartServer()
        {
            var kernel = new Kernel(hsfCache);
            var resolver = new NinjectResolver(kernel);

            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var selfHost = new SelfHost(ipString, resolver);
            selfHost.StartServer().Wait();
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

            hsfCache = new HighFrequencyFeedCache(feedLibraryUrl, highFrequencyRefreshSplit, highFrequencyRefreshPeriod);
        }

        void SetLowFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("LowFrequencyHttpWebRequestTimeout");
            var value = int.Parse(temp);
            AppSettings.SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(value);
        }
    }
}