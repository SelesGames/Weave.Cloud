using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class ServiceBusUpdater
    {
        IObservable<HighFrequencyFeed> updateQueue;
        QueueClient queueClient;
        EventLoopScheduler singleThreadScheduler = new EventLoopScheduler();

        public ServiceBusUpdater(QueueClient queueClient)
        {
            this.queueClient = queueClient;
        }

        public void Register(HighFrequencyFeed feed)
        {
            var feedUpdate = feed.FeedUpdate.Select(o => feed);

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

            updateQueue.ObserveOn(singleThreadScheduler).SubscribeOn(singleThreadScheduler).Subscribe(
                SafeOnHfFeedUpdate,
                exception =>
                {
                    DebugEx.WriteLine(exception);
                    Resubscribe();
                });
        }

        async void SafeOnHfFeedUpdate(HighFrequencyFeed update)
        {
            try
            {
                await OnHfFeedUpdate(update);
            }
            catch (Exception e)
            {
                //DebugEx.WriteLine(e);
            }
        }

        async Task OnHfFeedUpdate(HighFrequencyFeed feed)
        {
            await Task.Yield();
            using (var ms = new MemoryStream())
            {
                //ProtoBuf.Serializer.Serialize(ms, feed.News);
                //ms.Position = 0;
                //var message = new BrokeredMessage(ms, true);
                //message.ContentType = "application/protobuf";
                //message.Label = string.Format("{0}: {1}", feed.FeedId, feed.FeedUri);
                var message = new BrokeredMessage("hello world");
                queueClient.Send(message); // TODO: change to async version
            }
        }
    }
}
