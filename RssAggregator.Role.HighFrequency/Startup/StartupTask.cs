using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Diagnostics;
using System.ServiceModel;
using Weave.Mobilizer.Core.Controllers;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.HighFrequency.Startup
{
    internal class StartupTask
    {
        HighFrequencyFeedCache hsfCache;
        IKernel kernel;

        public void OnStart()
        {
            SetHighFrequencyValues();
            CreateAndStartServer();
            StartWCFService();
        }

        void CreateAndStartServer()
        {
            kernel = new Kernel(hsfCache);
        //    var resolver = new NinjectResolver(kernel);

        //    var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
        //    var ipString = string.Format("http://{0}", ip.ToString());
        //    Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

        //    var selfHost = new SelfHost(ipString, resolver);
        //    selfHost.StartServer().Wait();
        }

        void StartWCFService()
        {
            var baseAddress = String.Format(
                "net.tcp://{0}",
                RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint
                );

            //kernel.Bind<WeaveController>().ToSelf();
            var controller = kernel.Get<WeaveController>();
            var host = new ServiceHost(controller, new Uri(baseAddress));

            host.AddServiceEndpoint(typeof(IWeaveControllerService), new NetTcpBinding(SecurityMode.None), "api/weave");

            host.Open();
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
    }
}
