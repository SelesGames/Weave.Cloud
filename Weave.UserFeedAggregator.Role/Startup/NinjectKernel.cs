using Ninject;
using Weave.FeedUpdater.BusinessObjects.Cache;
using Weave.User.Service.InterRoleMessaging.Articles;

namespace Weave.User.Service.Role.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            Bind<IArticleQueueService>().To<ArticleQueueService>();
            Bind<ExpandedEntryCache>()
                .ToConstant(ExpandedEntryCacheFactory.CreateCache(100000))
                .InSingletonScope();
        }
    }
}