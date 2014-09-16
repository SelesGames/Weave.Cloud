using Common.WebApi.Handlers;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.WebApi;
using SelesGames.WebApi.SelfHost;
using StackExchange.Redis;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dependencies;
using Weave.Services.Redis.Ambient;
using Weave.User.BusinessObjects.Mutable.Cache;

namespace Weave.User.Service.Role.Startup
{
    internal class StartupTask
    {
        IKernel kernel;
        IDependencyResolver resolver;

        public async Task OnStart()
        {
            kernel = new NinjectKernel();
            resolver = new NinjectResolver(kernel);

            var clientConnection = Settings.StandardConnection;
            var pubsubConnection = Settings.PubsubConnection;

            kernel.Bind<ConnectionMultiplexer>().ToConstant(clientConnection).InSingletonScope();

            var userIndexCache = await UserIndexCacheFactory.CreateCacheAsync(
                clientConnection: clientConnection);

            kernel.Bind<UserIndexCache>().ToConstant(userIndexCache).InSingletonScope();
            
            CreateAndStartServer();
        }

        void CreateAndStartServer()
        {
            var ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint;
            var ipString = string.Format("http://{0}", ip.ToString());
            Trace.WriteLine(string.Format("**** IP ADDRESS: {0}", ipString));

            var config = SelfHost.Config;
            config.DependencyResolver = resolver;

            //config.MessageHandlers.Add(new InjectAcceptEncodingHandler("gzip"));
            config.MessageHandlers.Add(new InjectContentTypeHandler("application/json"));

            var cors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "*");
            config.EnableCors(cors);

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            SelfHost.StartServer(ipString);// host.StartServer(ipString);

            Trace.WriteLine("^&*^&*^&*^*&^  SERVER IS UP AND RUNNING!!!");
        }
    }
}
