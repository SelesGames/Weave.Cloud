using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using System.Web.Http.SelfHost;

namespace WebServiceAndCacheRedirect.WorkerRole.Startup
{
    internal class StartupTask
    {
        public void OnStart()
        {
            CreateAndStartServer();
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = new HttpConfig(ipString);
            new HttpSelfHostServer(config).OpenAsync().Wait();

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
