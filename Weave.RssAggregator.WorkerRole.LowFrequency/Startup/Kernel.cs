using Ninject;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.Core
{
    public class Kernel : StandardKernel
    {
        HighFrequencyFeedRssCache hfCache;

        public Kernel(HighFrequencyFeedRssCache hfCache)
        {
            this.hfCache = hfCache;
        }

        protected override void AddComponents()
        {
            base.AddComponents();

            Bind<HighFrequencyFeedRssCache>().ToConstant(hfCache).InSingletonScope();
        }
    }
}
