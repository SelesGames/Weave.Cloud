using Common.Azure.ServiceBus;
using Common.WebApi.Handlers;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using Weave.RssAggregator.LowFrequency;

namespace Weave.RssAggregator.WorkerRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        FeedCache hfCache;
        IDependencyResolver resolver;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel); 
            
            SetLowFrequencyValues();
            SetHighFrequencyValues();
            CreateAndStartServer();

            hfCache.InitializeAsync().Wait();
        }

        void SetLowFrequencyValues()
        {
            var temp = RoleEnvironment.GetConfigurationSettingValue("LowFrequencyHttpWebRequestTimeout");
            var value = int.Parse(temp);
            AppSettings.SetLowFrequencyHttpWebRequestTimeoutInMilliseconds(value);
        }

        void SetHighFrequencyValues()
        {
            var feedLibraryUrl = RoleEnvironment.GetConfigurationSettingValue("FeedLibraryUrl");

            hfCache = new FeedCache(
                feedLibraryUrl,
                kernel.Get<DbClient>(), 
                kernel.Get<SubscriptionConnector>());

            kernel.Bind<FeedCache>().ToConstant(hfCache);
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = new StandardHttpSelfHostConfiguration(ipString) { DependencyResolver = resolver };
            config.MessageHandlers.Add(new InjectAcceptHandler("application/protobuf"));
            config.MessageHandlers.Add(new InjectAcceptEncodingHandler("gzip"));
            new HttpSelfHostServer(config).OpenAsync().Wait();

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
