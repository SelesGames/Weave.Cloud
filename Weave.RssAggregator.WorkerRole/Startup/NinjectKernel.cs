using Ninject;
using RssAggregator.IconCaching;

namespace Weave.FeedUpdater.Service.Role.Startup
{
    public class NinjectKernel : StandardKernel
    {
        protected override void AddComponents()
        {
            base.AddComponents();

            Bind<NLevelIconUrlCache>().ToConstant(new StandardIconCache()).InSingletonScope();
        }
    }
}