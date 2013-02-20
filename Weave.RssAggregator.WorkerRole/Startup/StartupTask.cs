using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Diagnostics;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        HighFrequencyFeedUpdater hfCache;
        IDependencyResolver resolver;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel); 
            
            SetLowFrequencyValues();
            SetHighFrequencyValues();
            CreateAndStartServer();

            //hfCache.DoShit().Wait();

            //hfCache.StartFeedRefreshTimer();
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

            hfCache = new HighFrequencyFeedUpdater(
                feedLibraryUrl, 
                null, 
                highFrequencyRefreshSplit, 
                highFrequencyRefreshPeriod);

            kernel.Bind<HighFrequencyFeedUpdater>().ToMethod(_ => hfCache).InSingletonScope();
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
    }
}
