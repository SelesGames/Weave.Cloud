using System.Collections.Generic;

namespace Weave.RssAggregator.HighFrequency
{
    public class StandardProcessorChain : SequentialProcessorCollection<HighFrequencyFeedUpdate>
    {
        IAsyncProcessor<HighFrequencyFeedUpdate> processorCollection;

        public StandardProcessorChain()
            : base(CreateCollection())
        { }

        static IEnumerable<IAsyncProcessor<HighFrequencyFeedUpdate>> CreateCollection()
        {
            var processors = new IAsyncProcessor<HighFrequencyFeedUpdate>[] 
            {
                new RedirectResolver(),
                new BestImageSelectorProcessor(),
                new FeedUpdaterProcessor(),
                new PubSubUpdater(),
                new MobilizerOverride(),
            };
            return processors;
        }
    }
}