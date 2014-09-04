﻿using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    public class SequentialProcessor : IDisposable
    {
        IEnumerable<IProvider<IAsyncProcessor<HighFrequencyFeedUpdate>>> processorProviders;
        SubscriptionAggregator<Guid, HighFrequencyFeedUpdate> sub;
        IDisposable subHandle;

        public SequentialProcessor(IEnumerable<IProvider<IAsyncProcessor<HighFrequencyFeedUpdate>>> processorProviders)
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
            sub = new SubscriptionAggregator<Guid, HighFrequencyFeedUpdate>();

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

        async void SafeOnHfFeedUpdate(HighFrequencyFeedUpdate update)
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