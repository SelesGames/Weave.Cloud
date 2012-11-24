using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject.WebApi;
using System.Diagnostics;
using Weave.RssAggregator.HighFrequency;
using System.Linq;

namespace Weave.RssAggregator.WorkerRole.LowFrequency.Startup
{
    internal class StartupTask
    {
        HighFrequencyFeedCache hsfCache;

        public void OnStart()
        {
            SetLowFrequencyValues();
            SetHighFrequencyValues();
            SetInternalHFEndpoint();
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

        void SetInternalHFEndpoint()
        {
            var role = RoleEnvironment.Roles["RssAggregator.Role.HighFrequency"];
            if (role.Instances.Any())
            {
                var instance = role.Instances.First();
                var endpoint = instance.InstanceEndpoints.Single(o => o.Key == "Endpoint1");
                AppSettings.InternalHighFrequencyEndpoint = endpoint.Value.IPEndpoint.ToString();
                //isEndpointSet = true;
            }
        }

        //bool isEndpointSet = false;

        //void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e)
        //{
        //    if (isEndpointSet)
        //        return;

        //    if (e.Changes != null && e.Changes.OfType<RoleEnvironmentTopologyChange>().Any())
        //    {
        //        var role = RoleEnvironment.Roles["RssAggregator.Role.HighFrequency"];
        //        if (role.Instances.Any())
        //        {
        //            var instance = role.Instances.First();
        //            var endpoint = instance.InstanceEndpoints.Single(o => o.Key == "Endpoint1");
        //            AppSettings.InternalHighFrequencyEndpoint = endpoint.Value.IPEndpoint.ToString();
        //            isEndpointSet = true;
        //        }
        //    }
        //}
    }
}
