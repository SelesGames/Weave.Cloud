using Common.Azure.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class ServiceBusUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        TopicConnector topicConnector;

        public ServiceBusUpdater(TopicConnector topicConnector)
        {
            this.topicConnector = topicConnector;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            var client = await topicConnector.GetClient();

            using (var ms = new MemoryStream())
            {
                //ProtoBuf.Serializer.Serialize(ms, update);
                //ms.Position = 0;
                //var message = new BrokeredMessage(ms, false);
                //message.ContentType = "application/protobuf";
                //message.Label = string.Format("{0}: {1}", feed.FeedId, feed.FeedUri);
                var message = new BrokeredMessage();
                message.Properties["FeedId"] = update.FeedId;
                message.TimeToLive = TimeSpan.FromHours(24);
                await client.SendAsync(message);
                DebugEx.WriteLine("service bus processed: {0}", update.FeedUri);
            }
        }
    }
}
