using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using System.Web.Http.SelfHost;

namespace Weave.Mobilizer.Core.Web
{
    public class SelfHost
    {
        string url;
        IDependencyResolver resolver;

        public SelfHost(string url, IDependencyResolver resolver)
        {
            this.url = url;
            this.resolver = resolver;
        }

        public Task StartServer()
        {
            var config = new HttpSelfHostConfiguration(url);
            config.Configure(resolver);

            var server = new HttpSelfHostServer(config);
            return server.OpenAsync();
        }
    }
}
