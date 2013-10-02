using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;
using Weave.Identity.Service.WorkerRole.Controllers;

namespace Weave.Identity.Service.WorkerRole.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        IDependencyResolver resolver;

        public void OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel); 
            
            CreateAndStartServer();
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = new StandardHttpSelfHostConfiguration(ipString) { DependencyResolver = resolver };

            config.Routes.Clear();

            config.Routes.MapHttpRoute(
                name: "newroute",
                routeTemplate: "api/{controller}"
            );

            config.Routes.MapHttpRoute(
                "syncroute",
                routeTemplate: "api/identity/{controller}",
                defaults: new
                {
                    controller = typeof(SyncController)
                }
            );

            new HttpSelfHostServer(config).OpenAsync().Wait();

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
