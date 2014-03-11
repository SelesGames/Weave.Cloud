using Common.WebApi.Handlers;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using System.Diagnostics;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using System.Web.Http;
using System.Threading.Tasks;
using Weave.Mobilizer.Cache;
using System;
using Weave.Mobilizer.WorkerRole.Controllers;
using System.Web.Http.Cors;

namespace Weave.Mobilizer.WorkerRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        IDependencyResolver resolver;
        int localCacheTTL;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel);

            InitializeEnvironment();
            CreateAndStartServer();
        }
        
        void InitializeEnvironment()
        {
            if (!ReadConfigValues())
                throw new Exception("Unable to read the config values from Azure! (WebApiConfig.Register function)");

            var localCache = kernel.Get<LocalMemoryCache>();
            localCache.SetCacheTTLInMinutes(localCacheTTL);
        }

        bool ReadConfigValues()
        {
            var localCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("LocalCacheTTLInMinutes");
            if (!int.TryParse(localCacheTTLString, out localCacheTTL))
            {
                return false;
            }

            return true;
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = new StandardHttpSelfHostConfiguration(ipString) { DependencyResolver = resolver };

            config.Routes.MapHttpRoute(
                name: "InstapaperFormatterRouting",
                routeTemplate: "{controller}",
                defaults: new
                {
                    routTemplate = "ipf",
                    controller = typeof(IPFController),
                }
            );

            var cors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");
            config.EnableCors(cors);

            new HttpSelfHostServer(config).OpenAsync().Wait();

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
