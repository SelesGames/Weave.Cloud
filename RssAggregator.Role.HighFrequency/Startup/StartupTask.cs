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




//void CreateAndStartServer()
//{
//    //var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
//    //var ipString = string.Format("net.tcp://{0}", ip.ToString());
//    //Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

//    var sh = new ServiceHost(kernel.Get<HighFrequencyFeedRetriever>());

//    //sh.AddServiceEndpoint(
//    //   typeof(IHighFrequencyFeedRetriever), 
//    //   new NetTcpBinding(),
//    //   ipString);

//    var relayBinding = new NetTcpRelayBinding { ConnectionMode = TcpRelayConnectionMode.Hybrid };
//    relayBinding.Security.Mode = EndToEndSecurityMode.None;
//    //relayBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
//    //var relayBinding = new NetTcpRelayBinding();

//    sh.AddServiceEndpoint(
//       typeof(IHighFrequencyFeedRetriever),
//       relayBinding,
//       ServiceBusEnvironment.CreateServiceUri("sb", "weave-interop", "hf"))
//        .Behaviors.Add(new TransportClientEndpointBehavior
//        {
//            TokenProvider = TokenProvider.CreateSharedSecretTokenProvider("owner", "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=")
//        });

//    sh.Open();

//    Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
//}