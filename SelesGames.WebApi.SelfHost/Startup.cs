using Owin;

namespace SelesGames.WebApi.SelfHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = SelfHost.Config;
            app.UseWebApi(config);
        }
    }
}
