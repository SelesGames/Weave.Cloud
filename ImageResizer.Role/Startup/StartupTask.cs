using ImageResizer.Role.Controllers;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace ImageResizer.Role.Startup
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

            var config = SelfHost.Config;

            config.Routes.MapHttpRoute(
                name: "customRouting",
                routeTemplate: "api/{controller}",
                defaults: new
                {
                    controller = typeof(ImageController),
                });

            config.DependencyResolver = resolver;

            SelfHost.StartServer(ipString);

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}