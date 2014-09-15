using Ninject;
using StackExchange.Redis;
using Weave.User.Service.InterRoleMessaging.Articles;

namespace Weave.User.Service.Role.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            Bind<IArticleQueueService>().To<ArticleQueueService>();
        }
    }
}