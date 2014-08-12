using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class SequentialProcessor : IDisposable
    {
        IEnumerable<IProvider<ISequentialAsyncProcessor<FeedUpdate>>> processorProviders;
        SubscriptionAggregator<Guid, FeedUpdate> sub;
        IDisposable subHandle;

        public SequentialProcessor(IEnumerable<IProvider<ISequentialAsyncProcessor<FeedUpdate>>> processorProviders)
        {
            this.processorProviders = processorProviders;
            InitializeSubscription();
        }

        public void Register(HighFrequencyFeed feed)
        {
            sub.AddSubscription(feed.Id, feed.FeedUpdate);
        }

        void InitializeSubscription()
        {
            sub = new SubscriptionAggregator<Guid, FeedUpdate>();

            subHandle = sub
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(
                    SafeOnHfFeedUpdate,
                    exception =>
                    {
                        DebugEx.WriteLine(exception);
                        throw exception;
                    });
        }

        async void SafeOnHfFeedUpdate(FeedUpdate update)
        {
            foreach (var provider in processorProviders)
            {
                try
                {
                    var processor = provider.Get();
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
