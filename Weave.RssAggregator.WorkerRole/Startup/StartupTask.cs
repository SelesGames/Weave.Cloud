using Common.WebApi.Handlers;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using StackExchange.Redis;
using System.Diagnostics;
using System.Web.Http.Dependencies;
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
                kernel.Get<ConnectionMultiplexer>());

            kernel.Bind<FeedCache>().ToConstant(hfCache);
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = SelfHost.Config;
            config.DependencyResolver = resolver;
            config.MessageHandlers.Add(new InjectAcceptHandler("application/protobuf"));
            config.MessageHandlers.Add(new InjectAcceptEncodingHandler("gzip"));
            SelfHost.StartServer(ipString);

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
