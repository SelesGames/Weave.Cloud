using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject.WebApi;
using System;
using System.Diagnostics;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using Weave.RssAggregator.HighFrequency;
using Weave.RssAggregator.WorkerRole.Legacy;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    internal class StartupTask
    {
        HighFrequencyFeedCache hsfCache;
        IDependencyResolver resolver;

        public void OnStart()
        {
            SetLowFrequencyValues();
            SetHighFrequencyValues();
            CreateResolver();
            CreateAndStartServer();
            CreateAndStartLegacyServer();
        }

        void CreateResolver()
        {
            var kernel = new Kernel(hsfCache);
            resolver = new NinjectResolver(kernel);
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = new HttpConfig(ipString) { DependencyResolver = resolver };
            new HttpSelfHostServer(config).OpenAsync().Wait();

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }

        void CreateAndStartLegacyServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint2"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            //WcfEndpointCreator.CreateEndpoint(ipString, hsfCache);
            var config = new LegacyHttpConfig(ipString) { DependencyResolver = resolver };
            new HttpSelfHostServer(config).OpenAsync().Wait();

            Trace.WriteLine("^&*^&*^&*^*&^  LEGACY SERVER IS UP AND RUNNING!!!");
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
