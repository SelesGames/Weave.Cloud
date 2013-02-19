using System;
using System.Collections.Generic;

namespace Weave.RssAggregator.HighFrequency
{
    public class SequentialProcessor : IDisposable
    {
        IEnumerable<ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>> processors;
        SubscriptionAggregator<Guid, HighFrequencyFeedUpdateDto> sub;
        IDisposable subHandle;

        public SequentialProcessor(IEnumerable<ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>> processors)
        {
            this.processors = processors;
            InitializeSubscription();
        }

        public void Register(HighFrequencyFeed feed)
        {
            sub.AddSubscription(feed.FeedId, feed.FeedUpdate);
        }

        void InitializeSubscription()
        {
            sub = new SubscriptionAggregator<Guid, HighFrequencyFeedUpdateDto>();

            subHandle = sub.Subscribe(
                SafeOnHfFeedUpdate,
                exception =>
                {
                    DebugEx.WriteLine(exception);
                    throw exception;
                });
        }

        async void SafeOnHfFeedUpdate(HighFrequencyFeedUpdateDto update)
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
