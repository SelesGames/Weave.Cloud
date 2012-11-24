using Ninject;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.LowFrequency.Startup
{
    public class Kernel : StandardKernel
    {
        HighFrequencyFeedCache hfCache;

        public Kernel(HighFrequencyFeedCache hfCache)
        {
            this.hfCache = hfCache;
            Bind<HighFrequencyFeedCache>().ToMethod(_ => hfCache).InSingletonScope();
        }
    }
}
