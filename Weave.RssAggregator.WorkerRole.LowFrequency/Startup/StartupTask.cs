﻿using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject.WebApi;
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
            var feedLibraryUrl = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");
            hsfCache = new HighFrequencyFeedCache(feedLibraryUrl);
        }

        void SetLowFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("LowFrequencyHttpWebRequestTimeout");
            var value = int.Parse(temp);
            AppSettings.SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(value);
        }
    }
}
