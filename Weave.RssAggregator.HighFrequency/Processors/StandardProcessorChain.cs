using StackExchange.Redis;
using System.Collections.Generic;

namespace Weave.RssAggregator.HighFrequency
{
    public class StandardProcessorChain : SequentialProcessorCollection<HighFrequencyFeedUpdate>
    {
        IAsyncProcessor<HighFrequencyFeedUpdate> processorCollection;

        public StandardProcessorChain(ConnectionMultiplexer cm)
            : base(CreateCollection(cm))
        { }

        static IEnumerable<IAsyncProcessor<HighFrequencyFeedUpdate>> CreateCollection(ConnectionMultiplexer cm)
        {
            var processors = new IAsyncProcessor<HighFrequencyFeedUpdate>[] 
            {
                new RedirectResolver(),
                new BestImageSelectorProcessor(),
                new FeedUpdaterProcessor(cm),
                new PubSubUpdater(cm),
                new MobilizerOverride(),
            };
            return processors;
        }
    }
}
