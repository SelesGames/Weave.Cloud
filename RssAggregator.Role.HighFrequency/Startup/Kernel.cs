using Ninject;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.HighFrequency.Startup
{
    public class Kernel : StandardKernel
    {
        HighFrequencyFeedCache hfCache;

        public Kernel(HighFrequencyFeedCache hfCache)
        {
            this.hfCache = hfCache;
        }

        protected override void AddComponents()
        {
            base.AddComponents();
            Bind<HighFrequencyFeedCache>().ToConstant(hfCache).InSingletonScope();
        }
    }
}
