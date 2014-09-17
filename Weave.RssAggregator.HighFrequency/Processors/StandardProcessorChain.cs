using System.Collections.Generic;

namespace Weave.FeedUpdater.HighFrequency
{
    public class StandardProcessorChain : SequentialProcessorCollection<HighFrequencyFeedUpdate>
    {
        public StandardProcessorChain()
            : base(CreateCollection())
        { }

        static IEnumerable<IAsyncProcessor<HighFrequencyFeedUpdate>> CreateCollection()
        {
            var processors = new IAsyncProcessor<HighFrequencyFeedUpdate>[] 
            {
                new RedirectResolver(),
                new FeedIconUriUpdater(),
                new BestImageSelectorProcessor(),
                new FeedUpdaterProcessor(),
                new PubSubUpdater(),
                new MobilizerOverride(),
            };
            return processors;
        }
    }
}