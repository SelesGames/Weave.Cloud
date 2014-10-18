using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using System;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dependencies;
using Weave.Mobilizer.Cache;
using Weave.Mobilizer.WorkerRole.Controllers;

namespace Weave.Mobilizer.WorkerRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        IDependencyResolver resolver;
        //int localCacheTTL;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel);

            InitializeEnvironment();
            CreateAndStartServer();
        }
        
        void InitializeEnvironment()
        {
            //if (!ReadConfigValues())
            //    throw new Exception("Unable to read the config values from Azure! (WebApiConfig.Register function)");

            var localCache = kernel.Get<LocalMemoryCache>();
            //localCache.SetCacheTTLInMinutes(localCacheTTL);
        }

        //bool ReadConfigValues()
        //{
        //    var localCacheTTLString = RoleEnvironment.GetConfigurationSettingValue("LocalCacheTTLInMinutes");
        //    if (!int.TryParse(localCacheTTLString, out localCacheTTL))
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = SelfHost.Config;
            config.DependencyResolver = resolver;

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

            SelfHost.StartServer(ipString);

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
