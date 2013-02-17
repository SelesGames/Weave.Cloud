using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Weave.RssAggregator.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class SequentialProcessor
    {
        IDisposable existingSub;
        IObservable<Tuple<HighFrequencyFeed, List<Entry>>> updateQueue;
        IEnumerable<ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>> processors;

        public SequentialProcessor(IEnumerable<ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>> processors)
        {
            this.processors = processors;
        }

        public void Register(HighFrequencyFeed feed)
        {
            var feedUpdate = feed.FeedUpdate.Select(o => Tuple.Create(feed, o));

            if (updateQueue == null)
                updateQueue = feedUpdate;
            else
                updateQueue = updateQueue.Merge(feedUpdate);

            Resubscribe();
        }

        void Resubscribe()
        {
            if (updateQueue == null)
                return;

            if (existingSub != null)
                existingSub.Dispose();

            existingSub = updateQueue.Subscribe(
                SafeOnHfFeedUpdate,
                exception =>
                {
                    DebugEx.WriteLine(exception);
                    Resubscribe();
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
    }
}
