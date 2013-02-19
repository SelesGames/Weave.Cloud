using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Weave.RssAggregator.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class SequentialProcessor : IDisposable
    {
        IEnumerable<ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>> processors;
        SubscriptionAggregator<Guid, Tuple<HighFrequencyFeed, List<Entry>>> sub;
        IDisposable subHandle;

        public SequentialProcessor(IEnumerable<ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>> processors)
        {
            this.processors = processors;
            InitializeSubscription();
        }

        public void Register(HighFrequencyFeed feed)
        {
            var feedUpdate = feed.FeedUpdate.Select(o => Tuple.Create(feed, o));
            sub.AddSubscription(feed.FeedId, feedUpdate);
        }

        void InitializeSubscription()
        {
            sub = new SubscriptionAggregator<Guid, Tuple<HighFrequencyFeed, List<Entry>>>();

            subHandle = sub.Subscribe(
                SafeOnHfFeedUpdate,
                exception =>
                {
                    DebugEx.WriteLine(exception);
                    throw exception;
                });
        }

        async void SafeOnHfFeedUpdate(Tuple<HighFrequencyFeed, List<Entry>> update)
        {
            foreach (var processor in processors)
            {
                try
                {
                    await processor.ProcessAsync(update);
                    if (processor.IsHandledFully)
                        return;
                }
                catch (Exception e)
                {
                    DebugEx.WriteLine(e);
                    return;
                }
            }
        }

        public void Dispose()
        {
            if (subHandle != null)
                subHandle.Dispose();
        }
    }
}
