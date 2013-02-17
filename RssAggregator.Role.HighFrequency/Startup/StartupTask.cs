using Microsoft.ServiceBus;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Web.Http.Dependencies;
using Weave.RssAggregator.HighFrequency;

namespace RssAggregator.Role.HighFrequency
{
    internal class StartupTask
    {
        IKernel kernel;
        HighFrequencyFeedCache hfCache;
        IDependencyResolver resolver;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel); 
            
            SetHighFrequencyValues();
            //CreateAndStartServer();

            hfCache.StartFeedRefreshTimer();
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

            hfCache = new HighFrequencyFeedCache(
                feedLibraryUrl, 
                kernel.Get<SequentialProcessor>(), 
                highFrequencyRefreshSplit, 
                highFrequencyRefreshPeriod);

            kernel.Bind<HighFrequencyFeedCache>().ToMethod(_ => hfCache).InSingletonScope();
        }

        void CreateAndStartServer()
        {
            //var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            //var ipString = string.Format("net.tcp://{0}", ip.ToString());
            //Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var sh = new ServiceHost(kernel.Get<HighFrequencyFeedRetriever>());

            //sh.AddServiceEndpoint(
            //   typeof(IHighFrequencyFeedRetriever), 
            //   new NetTcpBinding(),
            //   ipString);

            var relayBinding = new NetTcpRelayBinding { ConnectionMode = TcpRelayConnectionMode.Hybrid };
            relayBinding.Security.Mode = EndToEndSecurityMode.None;
            //relayBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            //var relayBinding = new NetTcpRelayBinding();

            sh.AddServiceEndpoint(
               typeof(IHighFrequencyFeedRetriever),
               relayBinding,
               ServiceBusEnvironment.CreateServiceUri("sb", "weave-interop", "hf"))
                .Behaviors.Add(new TransportClientEndpointBehavior
                {
                    TokenProvider = TokenProvider.CreateSharedSecretTokenProvider("owner", "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=")
                });

            sh.Open();

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
